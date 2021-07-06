using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

using System;
using System.ComponentModel;
using System.Numerics;

namespace Neo.SmartContract
{
    [SupportedStandards("NEP-11")]
    [ContractPermission("*", "onNEP11Payment")]

    [DisplayName("Neo Knights")]
    [ManifestExtra("Author", "kokahunter")]
    [ManifestExtra("Email", "webmaster@neodepot.org")]
    [ManifestExtra("Description", "Neo Knights Contract")]   
    public class NeoKnights : Framework.SmartContract
    {
        [InitialValue("NQgpFDgZ9BRYWNc43JkHPpghpNUiLWFVTv", ContractParameterType.Hash160)]
        static readonly UInt160 Manager = default;
        public delegate void OnTransferDelegate(Neo.UInt160 from, Neo.UInt160 to, BigInteger amount, ByteString tokenId);
        public delegate void OnDoUpdateKnightDelegate(Neo.UInt160 from, ByteString tokenId);
        public delegate void OnDoVerifyKnightDelegate(string from, ByteString tokenId);
        public delegate void OnDoWithdrawDelegate(ByteString tokenId);

        [DisplayName("Transfer")]
        public static event OnTransferDelegate OnTransfer;

        [DisplayName("DoUpdateKnight")]
        public static event OnDoUpdateKnightDelegate OnDoUpdateKnight;

        [DisplayName("DoVerifyKnight")]
        public static event OnDoVerifyKnightDelegate OnDoVerifyKnight;

        [DisplayName("DoWithdraw")]
        public static event OnDoWithdrawDelegate OnDoWithdraw;

        private const byte Prefix_TotalSupply = 0x00;
        private const byte Prefix_Balance = 0x01;
        private const byte Prefix_VerifiedSupply = 0x02;
        private const byte Prefix_ClaimableGas = 0x03;
        private const byte Prefix_Deployed = 0x04;
        private const byte Prefix_Stats = 0x05;
        private const byte Prefix_TokenId = 0x10;
        private const byte Prefix_Token = 0x11;
        private const byte Prefix_AccountToken = 0x12;
        private const byte Prefix_NameToken = 0x13;
        private const byte Prefix_UrlApi = 0x21;
        private const byte Prefix_UrlTopWeek = 0x22;
        private const byte Prefix_FilterTop = 0x23;
        private const byte Prefix_UrlVerify1 = 0x24;
        private const byte Prefix_UrlVerify2 = 0x25;
        private const byte Prefix_FilterVerify = 0x26;
        private const ulong OneWeek = 7ul * 24 * 3600 * 1000;
        private const ulong OneMonth = 30ul * 24 * 3600 * 1000;

        public static byte Decimals() => 0;
        public static string Symbol() => "NKT";
        public static bool Verify() => false;

        [Safe]
        public static BigInteger TotalSupply() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_TotalSupply });

        [Safe]
        public static BigInteger DeployedStatus() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_Deployed });

        [Safe]
        public static BigInteger VerifiedSupply() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_VerifiedSupply });

        [Safe]
        public static BigInteger ClaimableGas() => (BigInteger)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_ClaimableGas });

        [Safe]
        public static BigInteger BalanceOf(Neo.UInt160 owner)
        {
            if (owner is null || !owner.IsValid)
                throw new Exception("The argument \"owner\" is invalid.");
            StorageMap balanceMap = new(Storage.CurrentContext, Prefix_Balance);
            return (BigInteger)balanceMap[owner];
        }
        [Safe]
        public static Neo.UInt160 OwnerOf(ByteString tokenId)
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);
            return token.Owner;
        }

        [Safe]
        public virtual Map<string, object> Properties(ByteString tokenId)
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);
            Map<string, object> map = new();
            map["name"] = token.Name;
            map["description"] = token.Description;
            return map;
        }
        [Safe]
        public virtual Map<string, object> PropertiesExt(ByteString tokenId)
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);
            Map<string, object> map = new();
            map["name"] = token.Name;
            map["description"] = token.Description;
            map["updateLock"] = token.UpdateLock;
            map["strength"] = token.Strength;
            map["knightPup"] = token.KnightPupKey64 == string.Empty ? "" : token.KnightPupKey64;
            map["gasAvail"] = token.GasAvail;
            map["verifiedLock"] = token.VerifiedLock;
            return map;
        }
        [Safe]
        public static Iterator Tokens()
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            return tokenMap.Find(FindOptions.KeysOnly | FindOptions.RemovePrefix);
        }
        [Safe]
        public virtual Map<string, object> Stats()
        {
            var gasBalance = GAS.BalanceOf(Runtime.ExecutingScriptHash);
            var claimableGas = (long)ClaimableGas();
            var gasAvail = gasBalance - claimableGas;
            var statsRaw = Storage.Get(Storage.CurrentContext, new byte[] { Prefix_Stats });
            Stats stats = (Stats)StdLib.Deserialize(statsRaw);
            Map<string, object> map = new();
            map["gasBurned"] = stats.GasBurned;
            map["gasDistributed"] = stats.GasDistributed;
            map["gasWithdrawn"] = stats.GasWithdrawn;
            map["totalSupply"] = TotalSupply();
            map["verifiedSupply"] = VerifiedSupply();
            map["gasBalance"] = gasBalance;
            map["gasAvail"] = gasAvail;
            return map;
        }
        
        [Safe]
        public static Iterator TokensOf(Neo.UInt160 owner)
        {
            if (owner is null || !owner.IsValid)
                throw new Exception("The argument \"owner\" is invalid");
            StorageMap accountMap = new(Storage.CurrentContext, Prefix_AccountToken);
            return accountMap.Find(owner, FindOptions.KeysOnly | FindOptions.RemovePrefix);
        }

        [Safe]
        public static Iterator TokenByName(string name)
        {
            if (name is null || name == string.Empty)
                throw new Exception("The argument \"name\" is invalid");
            StorageMap nameMap = new(Storage.CurrentContext, Prefix_NameToken);
            return nameMap.Find(name, FindOptions.KeysOnly | FindOptions.RemovePrefix);
        }

        [DisplayName("_deploy")]
        public static void Deploy(object data, bool update)
        {
            if (update) return;
            if (DeployedStatus() == 1) throw new Exception("Contract has been deployed.");
            
            StorageContext context = Storage.CurrentContext;
            Storage.Put(context, new byte[] { Prefix_Deployed }, "1");
            Storage.Put(context, new byte[] { Prefix_UrlApi }, "https://api.neodepot.org/api/");
            Storage.Put(context, new byte[] { Prefix_UrlTopWeek }, "https://www.reddit.com/r/NEO/top/.json?sort=top&t=week&limit=1");
            Storage.Put(context, new byte[] { Prefix_FilterTop }, "$.data.children[0].data.author");
            Storage.Put(context, new byte[] { Prefix_UrlVerify1 }, "https://www.reddit.com/user/");
            Storage.Put(context, new byte[] { Prefix_UrlVerify2 }, "/gilded/.json?limit=1");
            Storage.Put(context, new byte[] { Prefix_FilterVerify }, "$.data.children[0].data.selftext");
            var stats = new Stats()
            {
                GasBurned = 0,
                GasDistributed = 0,
                GasWithdrawn = 0        
            };
            Storage.Put(context, new byte[] { Prefix_Stats }, StdLib.Serialize(stats));
        }
        public static void Update(ByteString nefFile, string manifest)
        {
            if (!IsManager()) throw new Exception("No authorization.");
            ContractManagement.Update(nefFile, manifest, null);
        }
        public static void UpdateEndpoints(string endPoint, string urlNew)
        {
            if (!IsManager()) throw new Exception("No authorization.");
            StorageContext context = Storage.CurrentContext;
            if (endPoint == "UrlApi")
                Storage.Put(context, new byte[] { Prefix_UrlApi }, urlNew);
            if (endPoint == "UrlTopWeek")
                Storage.Put(context, new byte[] { Prefix_UrlTopWeek }, urlNew);
            if (endPoint == "FilterTop")
                Storage.Put(context, new byte[] { Prefix_FilterTop }, urlNew);
            if (endPoint == "UrlVerify1")
                Storage.Put(context, new byte[] { Prefix_UrlVerify1 }, urlNew);
            if (endPoint == "UrlVerify2")
                Storage.Put(context, new byte[] { Prefix_UrlVerify2 }, urlNew);
            if (endPoint == "FilterVerify")
                Storage.Put(context, new byte[] { Prefix_FilterVerify }, urlNew);
        }
        public static void Destroy()
        {
            if (!IsManager()) throw new Exception("No authorization.");
            ContractManagement.Destroy();
        }

        public static bool Transfer(Neo.UInt160 to, ByteString tokenId, object data)
        {
            if (to is null || !to.IsValid)
                throw new Exception("The argument \"to\" is invalid.");
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);
            Neo.UInt160 from = token.Owner;
            if (!Runtime.CheckWitness(from)) return false;
            if (from != to)
            {
                token.Owner = to;
                tokenMap[tokenId] = StdLib.Serialize(token);
                UpdateBalance(from, tokenId, -1);
                UpdateBalance(to, tokenId, +1);
            }
            PostTransfer(from, to, tokenId, data);
            return true;
        }
        public static void OnNEP17Payment(Neo.UInt160 from, int amount, object data)
        {
            if (Runtime.CallingScriptHash != GAS.Hash)
               throw new Exception("Only GAS is accepted");
            
            if (amount >= 10_00000000)
            {
                Iterator iterator = TokensOf(from);
                var count = 0;

                while (iterator.Next()){
                    count++;
                }
                
                if (count > 1)
                    throw new Exception("Cannot update or mint since you already own more than one NFT.");
                
                var urlApi = (string)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_UrlApi });
                var urlTop = (string)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_UrlTopWeek });
                var filter = (string)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_FilterTop });
                var url3 = (string)StdLib.Base58Encode(urlTop);
                var finalUrl = urlApi + url3;
                //oracle call for Update
                Oracle.Request(finalUrl, filter, "callbackTop", StdLib.Serialize(from), 1_00000000);
            }
        }
        public static void VerifyKnight(string knight, Neo.Cryptography.ECC.ECPoint pupKey)
        {
            if (!Runtime.CheckWitness(pupKey))
                throw new Exception($"Invalid pupKey {pupKey}.");
            
            ByteString tokenId = GetTokenIdByKnight(knight);
            
            if (tokenId == string.Empty)
                throw new Exception($"There is no knight named {knight}.");
            
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);

            if (token.KnightPupKey64 != string.Empty)
                throw new Exception("Already verified");

            var urlApi = (string)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_UrlApi }) + token.Name;
            urlApi = urlApi + "/";
            string url1 = (string)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_UrlVerify1 });
            string url2 = (string)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_UrlVerify2 });
            string filter = (string)Storage.Get(Storage.CurrentContext, new byte[] { Prefix_FilterVerify });
            string url3 = url1 + token.Name;
            url3 = url3 + url2;
            urlApi = urlApi + (string)StdLib.Base58Encode(url3);
            VerifyPayload payload = new()
            {
                PupKey64 = StdLib.Base64Encode(pupKey),
                TokenId = tokenId
            };
            Oracle.Request(urlApi, filter, "callbackVerify", StdLib.Serialize(payload) ,1_00000000);
        }

        public static void BurnGasOption(string knight)
        {
            var tokenId = GetTokenIdByKnight(knight);

            if (tokenId == string.Empty)
                throw new Exception($"There is no knight named {knight}.");

            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);
            
            if(token.KnightPupKey64 == string.Empty)
                throw new Exception($"Knight not verified!");
            if(!Runtime.CheckWitness((Neo.Cryptography.ECC.ECPoint)StdLib.Base64Decode(token.KnightPupKey64)))
                throw new Exception("Not authorized.");

            if (token.VerifiedLock > Runtime.Time)
                throw new Exception("Burn only once per month and knight!");

            var gasBalance = (long)GAS.BalanceOf(Runtime.ExecutingScriptHash);
            var claimableGas = (long)ClaimableGas();
            var gasAvail = gasBalance - claimableGas;
            if (gasAvail <= 0)
                throw new Exception("Insufficient gas available.");
            
            //Reset time lock
            token.VerifiedLock = Runtime.Time + OneMonth;
            tokenMap[tokenId] = StdLib.Serialize(token);
            
            UpdateStats("burn", gasAvail);
            GAS.Refuel(Runtime.ExecutingScriptHash, gasAvail);
            Runtime.BurnGas(gasAvail);
        }
        public static void DistributeGasOption(string knight)
        {
            var tokenId = GetTokenIdByKnight(knight);

            if (tokenId == string.Empty)
                throw new Exception($"There is no knight named {knight}.");

            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);
            
            if(token.KnightPupKey64 == string.Empty)
                throw new Exception($"Knight not verified!");
            if (!Runtime.CheckWitness((Neo.Cryptography.ECC.ECPoint)StdLib.Base64Decode(token.KnightPupKey64)))
                throw new Exception("Not authorized.");

            if (token.VerifiedLock > Runtime.Time)
                throw new Exception("Distribute only once per month and knight!");
            
            //Distribute GAS equally to all knights
            var gasBalance = (long)GAS.BalanceOf(Runtime.ExecutingScriptHash);
            var claimableGas = (long)ClaimableGas();
            var gasAvail = gasBalance - claimableGas;
            if (gasAvail <= 0)
                throw new Exception("Insufficient gas available.");
            
            var totalSupply = (long)TotalSupply();
            if (totalSupply == 0)
                throw new Exception("No knights to distribute to.");
            
            var gasFraction = gasAvail / totalSupply;

            if (gasFraction == 0)
                throw new Exception("Not enough gas for all knights.");
            
            if(totalSupply > 1)
            {
                Iterator allTokens = Tokens();
                while (allTokens.Next())
                {
                    if((ByteString)allTokens.Value != tokenId)
                    {
                        var tokens = (TokenState)StdLib.Deserialize(tokenMap[(ByteString)allTokens.Value]);
                        tokens.GasAvail += gasFraction;
                        tokenMap[(ByteString)allTokens.Value] = StdLib.Serialize(tokens);
                    }
                } 
            }

            token.GasAvail += gasFraction;
            //Reset time lock for caller 
            token.VerifiedLock = Runtime.Time + OneMonth;
            tokenMap[tokenId] = StdLib.Serialize(token);      
            UpdateStats("distribute", gasAvail);    
            UpdateClaimableGas(+gasAvail);         
        }
        public static void Withdraw(string knight, UInt160 toAddr)
        {
            if (!toAddr.IsValid)
                throw new Exception($"Not valid toAddr {toAddr.ToString()}.");
            
            var tokenId = GetTokenIdByKnight(knight);

            if (tokenId == string.Empty)
                throw new Exception($"There is no knight named {knight}.");

            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);

            if (!Runtime.CheckWitness((Neo.Cryptography.ECC.ECPoint)StdLib.Base64Decode(token.KnightPupKey64)))
                throw new Exception("Not authorized.");

            if (token.GasAvail <= 0)
                throw new Exception($"No gas available for {knight}.");

            var amount = token.GasAvail;
            token.GasAvail = 0;
            tokenMap[tokenId] = StdLib.Serialize(token);
            UpdateStats("withdraw", amount);
            UpdateClaimableGas(-amount);  
            GAS.Transfer(Runtime.ExecutingScriptHash, toAddr, amount, null);
            OnDoWithdraw(tokenId);
        }
        public static void CallbackTop(string url, ByteString userdata, OracleResponseCode code, string result)
        {
            if (Runtime.CallingScriptHash != Oracle.Hash) 
                throw new Exception("Unauthorized!");
            if (code != OracleResponseCode.Success) 
                throw new Exception("Oracle response failure with code " + (byte)code);

            TokenState token = new()
            {
                Owner = UInt160.Zero,
                Name = string.Empty,
                Description = string.Empty,
                UpdateLock = Runtime.Time,
                Strength = 0,
                KnightPupKey64 = string.Empty,
                GasAvail = 0,
                VerifiedLock = Runtime.Time              
            };
            object ret = StdLib.JsonDeserialize(result);
            object[] arr = (object[])ret;
            string knight = (string)arr[0];
            string title = "hello neo knight";

            if (knight == string.Empty || knight == "[deleted]") 
                throw new Exception("Result was empty or account is deleted.");
            
            //get userdata
            var from = (Neo.UInt160)StdLib.Deserialize(userdata);

            //check if knight already exists
            ByteString tokenId = GetTokenIdByKnight(knight);
            
            //Knight exists
            if (tokenId != string.Empty)
            {
                StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
                token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);

                //disable for testing purpose
                if (token.UpdateLock > Runtime.Time)
                    throw new Exception("Update only once per week and knight!");

                //Update knight's strength
                token.Strength++;
                token.UpdateLock = Runtime.Time + OneWeek;
                token.Description = title;
                tokenMap[tokenId] = StdLib.Serialize(token);
                OnDoUpdateKnight(from, tokenId);
            }
            //Knight has not been seen yet
            else 
            {
                //Check if sender has one knight already
                Iterator iterator = TokensOf(from);
                var count = 0;
                while (iterator.Next()){
                    count++;
                }

                if (count > 0)
                    throw new Exception("Cannot empower more than one knight.");

                tokenId = NewTokenId();

                token.Owner = from;
                token.Name = knight;
                token.Description = title;
                token.UpdateLock = Runtime.Time + OneWeek;
                token.VerifiedLock = Runtime.Time + OneMonth;
                token.Strength = 1;
                token.KnightPupKey64 = string.Empty;
                token.GasAvail = 0;

                Mint(tokenId, token);
            }            
        }
        public static void CallbackVerify(string url, ByteString userdata, OracleResponseCode code, string result)
        {
            if (Runtime.CallingScriptHash != Oracle.Hash) 
                throw new Exception("Unauthorized!");
            if (code != OracleResponseCode.Success) 
                throw new Exception("Oracle response failure with code " + (byte)code);
            
            object ret = StdLib.JsonDeserialize(result);
            object[] arr = (object[])ret;
            byte[] message = (byte[])arr[0];
            var tmp1 = message[..89];
            var tmp2 = (ByteString)tmp1;
            var signature = (ByteString)StdLib.Base64Decode(tmp2);

            VerifyPayload payload = (VerifyPayload)StdLib.Deserialize(userdata);
            Neo.Cryptography.ECC.ECPoint pupKeyCaller = (Neo.Cryptography.ECC.ECPoint)StdLib.Base64Decode(payload.PupKey64);

            var checkSig = CryptoLib.VerifyWithECDsa((ByteString)"NeoKnights", pupKeyCaller, signature, NamedCurve.secp256r1);

            if (!checkSig)
                throw new Exception("You shall not pass.");
            
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[payload.TokenId]);
            token.KnightPupKey64 = payload.PupKey64;
            tokenMap[payload.TokenId] = StdLib.Serialize(token);
            UpdateVerifiedSupply(+1);
            OnDoVerifyKnight(payload.PupKey64, payload.TokenId);
        }
        private static ByteString GetTokenIdByKnight(string name)
        {
            var tokenByName = TokenByName(name);
            var count = 0;
            ByteString existingTokenId = string.Empty;
            while (tokenByName.Next()){
                count ++;
                existingTokenId = (ByteString)tokenByName.Value;
            }
            if (count > 1)
                throw new Exception("This should never happen!");

            return existingTokenId;          
        }
        private static bool ValidateAddress(UInt160 address) => address.IsValid && !address.IsZero;
        private static ByteString NewTokenId()
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_TokenId };
            ByteString id = Storage.Get(context, key);
            Storage.Put(context, key, (BigInteger)id + 1);
            ByteString data = Runtime.ExecutingScriptHash;
            if (id is not null) data += id;
            return CryptoLib.Sha256(data);
        }

        private static void Mint(ByteString tokenId, TokenState token)
        {
            StorageMap nameMap = new(Storage.CurrentContext, Prefix_NameToken);
            ByteString key = token.Name + tokenId;
            nameMap.Put(key, 0);
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            tokenMap[tokenId] = StdLib.Serialize(token);
            UpdateBalance(token.Owner, tokenId, +1);
            UpdateTotalSupply(+1);
            PostTransfer(null, token.Owner, tokenId, null);
        }

        private static void Burn(ByteString tokenId)
        {
            StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
            TokenState token = (TokenState)StdLib.Deserialize(tokenMap[tokenId]);
            StorageMap nameMap = new(Storage.CurrentContext, Prefix_NameToken);
            ByteString key = token.Name + tokenId;
            nameMap.Delete(key);
            tokenMap.Delete(tokenId);
            UpdateBalance(token.Owner, tokenId, -1);
            UpdateTotalSupply(-1);
            UpdateVerifiedSupply(-1);
            PostTransfer(token.Owner, null, tokenId, null);
        }
        private static void UpdateClaimableGas(BigInteger amount)
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_ClaimableGas };
            BigInteger claimableGas = (BigInteger)Storage.Get(context, key);
            claimableGas += amount;
            if (claimableGas < 0)
                throw new Exception("Claimable gas less zero.");
            Storage.Put(context, key, claimableGas);
        }
        private static void UpdateStats(string type, BigInteger amount)
        {
            var prefix = new byte[] { Prefix_Stats };
            var statsRaw = Storage.Get(Storage.CurrentContext, prefix);
            Stats stats = (Stats)StdLib.Deserialize(statsRaw);
            if(type == "withdraw")
                stats.GasWithdrawn += (long)amount;
            if(type == "distribute")
                stats.GasDistributed += (long)amount;
            if(type == "burn")
                stats.GasBurned += (long)amount;
            Storage.Put(Storage.CurrentContext, prefix, StdLib.Serialize(stats));
        }
        private static void UpdateBalance(Neo.UInt160 owner, ByteString tokenId, int increment)
        {
            UpdateBalance(owner, increment);
            StorageMap accountMap = new(Storage.CurrentContext, Prefix_AccountToken);
            ByteString key = owner + tokenId;
            if (increment > 0)
                accountMap.Put(key, 0);
            else
                accountMap.Delete(key);
        }
        private static bool UpdateBalance(Neo.UInt160 owner, BigInteger increment)
        {
            StorageMap balanceMap = new(Storage.CurrentContext, Prefix_Balance);
            BigInteger balance = (BigInteger)balanceMap[owner];
            balance += increment;
            if (balance < 0) return false;
            if (balance.IsZero)
                balanceMap.Delete(owner);
            else
                balanceMap.Put(owner, balance);
            return true;
        }
        private static void UpdateTotalSupply(BigInteger increment)
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_TotalSupply };
            BigInteger totalSupply = (BigInteger)Storage.Get(context, key);
            totalSupply += increment;
            Storage.Put(context, key, totalSupply);
        }
        private static void UpdateVerifiedSupply(BigInteger increment)
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_VerifiedSupply };
            BigInteger totalSupply = (BigInteger)Storage.Get(context, key);
            totalSupply += increment;
            Storage.Put(context, key, totalSupply);
        }
        private static void PostTransfer(Neo.UInt160 from, Neo.UInt160 to, ByteString tokenId, object data)
        {
            OnTransfer(from, to, 1, tokenId);
            if (to is not null && ContractManagement.GetContract(to) is not null)
                Contract.Call(to, "onNEP11Payment", CallFlags.All, from, 1, tokenId, data);
        }
        private static bool IsManager() => Runtime.CheckWitness(Manager);
    }
}
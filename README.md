# Neo Knights

Self-sustaining protocol to support outstanding community members and the global N3 network.

Neo-Knights keeps track of top reddit contributors while giving them options to manage incoming donations.

Contract hash: 0x0af362def0832d8616ce84d3b9960785fd780a10

Contract address: NMNnYUXuMYvU3UcCjKCr9g1gRtcuFnJQjE

Currently live on N3 RC3 testnet.

Frontend: https://neo-knights.github.io/

## About

This project is entering the Neo Frontier Launchpad.

The goals are to enable and strengthen engagement and funding throughout the N3 community. Neo Knights should work in a decentralized manner, nevertheless it's value is yet to be proven. If there is community engagement, launching on N3 MainNet should improve the protocol based on community suggestions:

* Deploy different security roles, based on strenght
* Enable a multiSig for Updates and parameter changes, based on verified Knight's public keys
* Support more actions beside distribute and burn
* ...

Any input is welcome, please feel free to open an issue on Github.

## Methods

### decimals

```json
{
	"name": "decimals",
	"safe": false,
	"returntype": "Integer",
	"parameters": []
}
```

Standard Nep-11.

### symbol

```json
{
	"name": "symbol",
	"safe": false,
	"returntype": "String",
	"parameters": []
}
```

Standard Nep-11.

### verify

```json
{
	"name": "verify",
	"safe": false,
	"returntype": "Boolean",
	"parameters": []
}
```

Always false.

### totalSupply

```json
{
	"name": "totalSupply",
	"safe": true,
	"returntype": "Integer",
	"parameters": []
}
```

Standard Nep-11.

### deployedStatus

```json
{
	"name": "deployedStatus",
	"safe": true,
	"returntype": "Integer",
	"parameters": []
}
```

Returns `1` if contract has been deployed.

### verifiedSupply

```json
{
	"name": "verifiedSupply",
	"safe": true,
	"returntype": "Integer",
	"parameters": []
}
```

Returns the number of verified knights.

### claimableGas

```json
{
	"name": "claimableGas",
	"safe": true,
	"returntype": "Integer",
	"parameters": []
}
```

Returns the amount of GAS that has been distributed and not withdrawn yet.

### balanceOf

```json
{
	"name": "balanceOf",
	"safe": true,
	"returntype": "Integer",
	"parameters": [
		{
			"name": "owner",
			"type": "Hash160"
		}
	]
}
```

Standard Nep-11.

### ownerOf

```json
{
	"name": "ownerOf",
	"safe": true,
	"returntype": "Hash160",
	"parameters": [
		{
			"name": "tokenId",
			"type": "ByteArray"
		}
	]
}
```

Standard Nep-11.

### properties

```json
{
	"name": "properties",
	"safe": true,
	"returntype": "Map",
	"parameters": [
		{
			"name": "tokenId",
			"type": "ByteArray"
		}
	]
}
```

Standard Nep-11.

### propertiesExt

```json
{
	"name": "propertiesExt",
	"safe": true,
	"returntype": "Map",
	"parameters": [
		{
			"name": "tokenId",
			"type": "ByteArray"
		}
	]
}
```

Extended properties for serving web app.

### tokens

```json
{
	"name": "tokens",
	"safe": true,
	"returntype": "InteropInterface",
	"parameters": []
}
```

Standard Nep-11.

### stats

```json
{
	"name": "stats",
	"safe": true,
	"returntype": "Map",
	"parameters": []
}
```

Returns basic stats for serving web app:

```c#
public class Stats
{
	public long GasBurned;
	public long GasDistributed;
	public long GasWithdrawn;
}
```

### tokensOf

```json
{
	"name": "tokensOf",
	"safe": true,
	"returntype": "InteropInterface",
	"parameters": [
		{
			"name": "owner",
			"type": "Hash160"
		}
	]
}
```

Standard Nep-11.

### tokenByName

```json
{
	"name": "tokenByName",
	"safe": true,
	"returntype": "InteropInterface",
	"parameters": [
		{
			"name": "name",
			"type": "String"
		}
	]
}
```

Returns the the `tokenId` for the name. The method should never return more than one `tokenId` for any given name.

### _deploy

```json
{
	"name": "_deploy",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "data",
			"type": "Any"
		},
		{
			"name": "update",
			"type": "Boolean"
		}
	]
}
```

`deploy` sets initial values for `deployedStatus`, `url` for oracle calls and `stats`.

### update

```json
{
	"name": "update",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "nefFile",
			"type": "ByteArray"
		},
		{
			"name": "manifest",
			"type": "String"
		}
	]
}
```

Can only be called by 'Manager'. This method will be removed on MainNet.

### updateEndpoints

```json
{
	"name": "updateEndpoints",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "endPoint",
			"type": "String"
		},
		{
			"name": "urlNew",
			"type": "String"
		}
	]
}
```

Can only be called by 'Manager'. This method will be removed on MainNet.

### destroy

```json
{
	"name": "destroy",
	"safe": false,
	"returntype": "Void",
	"parameters": []
}
```

Can only be called by 'Manager'. This method will be removed on MainNet.

### transfer

```json
{
	"name": "transfer",
	"safe": false,
	"returntype": "Boolean",
	"parameters": [
		{
			"name": "to",
			"type": "Hash160"
		},
		{
			"name": "tokenId",
			"type": "ByteArray"
		},
		{
			"name": "data",
			"type": "Any"
		}
	]
}
```

Standard Nep-11.

### onNEP17Payment

```json
{
	"name": "onNEP17Payment",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "from",
			"type": "Hash160"
		},
		{
			"name": "amount",
			"type": "Integer"
		},
		{
			"name": "data",
			"type": "Any"
		}
	]
}
```

This contract only accepts GAS.

Any GAS transfer of at least 10 GAS will call `Oracle.Request` with the parameters:
* Url*1: `https://www.reddit.com/r/NEO/top/.json?sort=top&t=week&limit=1`
* Callback function: `callbackTop`
* Filter: `$.data.children[0].data.author`
* Userdata: `Neo.UInt160 from`

*1
> Please note: In this pre-version the Oracle Request is relayed through https://github.com/Neo-Knights/neo-knights-api, since the direct call does not work on N3 RC3.

### verifyKnight

```json
{
	"name": "verifyKnight",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "knight",
			"type": "String"
		},
		{
			"name": "pupKey",
			"type": "PublicKey"
		}
	]
}
```

`verifyKnight` must be called in order to verify as a knight and get access to `burnGasOption`, `distributeGasOption` and `withdraw`.

For successfully verifying, conditions needs to be met:
* Must be registered as knight
* Must have a gilded post, which text content starting with a base64 encoded signed message

Sign the message "NeoKnights" with your N3 account. Call `verifyKnight` with the same account.

C# example (requires nuget package `Neo.Network.RPC.RpcClient`):
```c#
var privateHex = "private key hex string";
var keyPair = Utility.GetKeyPair(privateHex);
byte[] message = System.Text.Encoding.Default.GetBytes("NeoKnights");
byte[] signature = Crypto.Sign(message, keyPair.PrivateKey, keyPair.PublicKey.EncodePoint(false).Skip(1).ToArray());
var verifySignature = Crypto.VerifySignature(message, signature, keyPair.PublicKey);
var signature64 = StdLib.Base64Encode(signature);
```

The gilded post will be fetched from `https://www.reddit.com/user/{username}/gilded/.json?limit=1`

### burnGasOption

```json
{
	"name": "burnGasOption",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "knight",
			"type": "String"
		}
	]
}
```

Calling this as a verified knight will  burn all available GAS from the contract balance.

Available GAS = `GAS.BalanceOf(NeoKnightsContract)` - `claimableGas()`.

### distributeGasOption

```json
{
	"name": "distributeGasOption",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "knight",
			"type": "String"
		}
	]
}
```

Calling this as a verified knight will distribute all available GAS equally to all registered knights.

### withdraw

```json
{
	"name": "withdraw",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "knight",
			"type": "String"
		},
		{
			"name": "toAddr",
			"type": "Hash160"
		}
	]
}
```

Calling this as a verified knight will withdraw the knight's available GAS balance to any address specified by the knight.

### callbackTop

```json
{
	"name": "callbackTop",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "url",
			"type": "String"
		},
		{
			"name": "userdata",
			"type": "ByteArray"
		},
		{
			"name": "code",
			"type": "Integer"
		},
		{
			"name": "result",
			"type": "String"
		}
	]
}
```

`callback` gets the author of the top post of the week from the /r/NEO/ subreddit. It then checks against the name (author) if the knight has already been minted.

If already minted, update the existing knight and add +1 strength to the NFT.

Mint a new Neo Knight as NFT, if there is no knight found with the name (author).

### callbackVerify

```json
{
	"name": "callbackVerify",
	"safe": false,
	"returntype": "Void",
	"parameters": [
		{
			"name": "url",
			"type": "String"
		},
		{
			"name": "userdata",
			"type": "ByteArray"
		},
		{
			"name": "code",
			"type": "Integer"
		},
		{
			"name": "result",
			"type": "String"
		}
	]
}
```

`callbackVerify` gets the `selftext` field of the gilded reddit post from the `knight`. Username and puplic key are serialized into the `VerifyPayload` from the `verifyKnight` function).

Verifying the knight:
1. Get the base64 encoded signed message from the oracle response
2. `Base64Decode` the message
3. Verify against public key from `VerifyPayload` --> `CryptoLib.VerifyWithECDsa((ByteString)"NeoKnights", pupKeyCaller, signature, NamedCurve.secp256r1)`
4. If signed message is valid, update verified supply and store public key in NFT data

The Knight is now verified and is allowed to call `burnGasOption`, `distributeGasOption` and `withdraw`.

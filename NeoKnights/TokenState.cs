namespace Neo.SmartContract
{
    public class TokenState 
    {
        public Neo.UInt160 Owner;
        public string Name;
        public string Description;
        public int Strength;
        public ulong UpdateLock;
        public string KnightPupKey64;
        public long GasAvail;
        public ulong VerifiedLock;
    }
}
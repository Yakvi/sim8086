namespace sim8086
{
    public static class BitwiseUtilities
    {
        public static bool GetBit(this byte b, int position)
        {
            if (position < 0 || position > 7) return false;
            return (b & (1 << position)) != 0;
        }
        
        public static bool GetBit(this short b, int position)
        {
            if (position < 0 || position > 15) return false;
            return (b & (1 << position)) != 0;
        }
    }
}
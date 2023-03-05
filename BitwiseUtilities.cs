namespace sim8086
{
    public static class BitwiseUtilities
    {
        public static bool GetBit(this byte b, int position)
        {
            if (position < 0 || position > 7) return false;
            var p = 7 - position;
            return (b & (1 << p)) != 0;
        }
    }
}
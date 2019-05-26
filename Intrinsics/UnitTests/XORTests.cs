using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class XORTests
    {
        [TestMethod]
        public void ManualTest()
        {
            Intrinsics.XOR xor = new Intrinsics.XOR();

            byte[] oldScreen;
            byte[] newScreen;
            byte[] difference;

            foreach (object[] data in xor.DataSourceBytes())
            {
                oldScreen = (byte[])data[0];
                newScreen = (byte[])data[1];
                difference = (byte[])data[2];

                xor.Manual(oldScreen, newScreen, difference);

                for (int position = 0; position < difference.Length; position++)
                    Assert.AreEqual(difference[position], (byte)(oldScreen[position] ^ newScreen[position]), $"Invalid byte at position {position}: 0x{difference[position].ToString("x02")} != 0x{(oldScreen[position] ^ newScreen[position]).ToString("x02")}.");
            }
        }

        [TestMethod]
        public void BytePointersTest()
        {
            Intrinsics.XOR xor = new Intrinsics.XOR();

            byte[] oldScreen;
            byte[] newScreen;
            byte[] difference;

            foreach (object[] data in xor.DataSourceBytes())
            {
                oldScreen = (byte[])data[0];
                newScreen = (byte[])data[1];
                difference = (byte[])data[2];

                xor.BytePointers(oldScreen, newScreen, difference);

                for (int position = 0; position < difference.Length; position++)
                    Assert.AreEqual(difference[position], (byte)(oldScreen[position] ^ newScreen[position]), $"Invalid byte at position {position}: 0x{difference[position].ToString("x02")} != 0x{(oldScreen[position] ^ newScreen[position]).ToString("x02")}.");
            }
        }

        [TestMethod]
        public void LongPointersTest()
        {
            Intrinsics.XOR xor = new Intrinsics.XOR();

            byte[] oldScreen;
            byte[] newScreen;
            byte[] difference;

            foreach (object[] data in xor.DataSourceBytes())
            {
                oldScreen = (byte[])data[0];
                newScreen = (byte[])data[1];
                difference = (byte[])data[2];

                xor.LongPointers(oldScreen, newScreen, difference);

                for (int position = 0; position < difference.Length; position++)
                    Assert.AreEqual(difference[position], (byte)(oldScreen[position] ^ newScreen[position]), $"Invalid byte at position {position}: 0x{difference[position].ToString("x02")} != 0x{(oldScreen[position] ^ newScreen[position]).ToString("x02")}.");
            }
        }

        [TestMethod]
        public void IntrinsicsAVXTest()
        {
            Intrinsics.XOR xor = new Intrinsics.XOR();

            byte[] oldScreen;
            byte[] newScreen;
            byte[] difference;

            foreach (object[] data in xor.DataSourceBytes())
            {
                oldScreen = (byte[])data[0];
                newScreen = (byte[])data[1];
                difference = (byte[])data[2];

                xor.IntrinsicsAVX(oldScreen, newScreen, difference);

                for (int position = 0; position < difference.Length; position++)
                    Assert.AreEqual(difference[position], (byte)(oldScreen[position] ^ newScreen[position]), $"Invalid byte at position {position}: 0x{difference[position].ToString("x02")} != 0x{(oldScreen[position] ^ newScreen[position]).ToString("x02")}.");
            }
        }

        [TestMethod]
        public void IntrinsicsAVX2Test()
        {
            Intrinsics.XOR xor = new Intrinsics.XOR();

            byte[] oldScreen;
            byte[] newScreen;
            byte[] difference;

            foreach (object[] data in xor.DataSourceBytes())
            {
                oldScreen = (byte[])data[0];
                newScreen = (byte[])data[1];
                difference = (byte[])data[2];

                xor.IntrinsicsAVX2(oldScreen, newScreen, difference);

                for (int position = 0; position < difference.Length; position++)
                    Assert.AreEqual(difference[position], (byte)(oldScreen[position] ^ newScreen[position]), $"Invalid byte at position {position}: 0x{difference[position].ToString("x02")} != 0x{(oldScreen[position] ^ newScreen[position]).ToString("x02")}.");
            }
        }

        [TestMethod]
        public void SIMDBytesTest()
        {
            Intrinsics.XOR xor = new Intrinsics.XOR();

            byte[] oldScreen;
            byte[] newScreen;
            byte[] difference;

            foreach (object[] data in xor.DataSourceBytes())
            {
                oldScreen = (byte[])data[0];
                newScreen = (byte[])data[1];
                difference = (byte[])data[2];

                xor.SIMDBytes(oldScreen, newScreen, difference);

                for (int position = 0; position < difference.Length; position++)
                    Assert.AreEqual(difference[position], (byte)(oldScreen[position] ^ newScreen[position]), $"Invalid byte at position {position}: 0x{difference[position].ToString("x02")} != 0x{(oldScreen[position] ^ newScreen[position]).ToString("x02")}.");
            }
        }

        [TestMethod]
        public void SIMDLongsTest()
        {
            Intrinsics.XOR xor = new Intrinsics.XOR();

            long[] oldScreen;
            long[] newScreen;
            long[] difference;

            foreach (object[] data in xor.DataSourceLongs())
            {
                oldScreen = (long[])data[0];
                newScreen = (long[])data[1];
                difference = (long[])data[2];

                xor.SIMDLong(oldScreen, newScreen, difference);

                for (int position = 0; position < difference.Length; position++)
                    Assert.AreEqual(difference[position], oldScreen[position] ^ newScreen[position], $"Invalid long at position {position}: 0x{difference[position].ToString("x016")} != 0x{(oldScreen[position] ^ newScreen[position]).ToString("x016")}.");
            }
        }
    }
}

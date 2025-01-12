//
// UnicodeEncodingTest.cs - NUnit Test Cases for System.Text.UnicodeEncoding
//
// Author:
//     Patrick Kalkman  kalkman@cistron.nl
//
// (C) 2003 Patrick Kalkman
// 
using NUnit.Framework;
using System;
using System.Text;

namespace MonoTests.System.Text
{
	[TestFixture]
	public class UnicodeEncodingTest 
	{
		[Test]
		public void IsBrowserDisplay ()
		{
			UnicodeEncoding enc = new UnicodeEncoding ();
			Assert.IsFalse (enc.IsBrowserDisplay);
		}

		[Test]
		public void IsBrowserSave ()
		{
			UnicodeEncoding enc = new UnicodeEncoding ();
			Assert.IsTrue (enc.IsBrowserSave);
		}

		[Test]
		public void IsMailNewsDisplay ()
		{
			UnicodeEncoding enc = new UnicodeEncoding ();
			Assert.IsFalse (enc.IsMailNewsDisplay);
		}

		[Test]
		public void IsMailNewsSave ()
		{
			UnicodeEncoding enc = new UnicodeEncoding ();
			Assert.IsFalse (enc.IsMailNewsSave);
		}

                [Test]
                public void TestEncodingGetBytes1()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        byte[] UniBytes;
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (); //little-endian
                        UniBytes = UnicodeEnc.GetBytes (Unicode);
                        
                        Assert.AreEqual (0xA0, UniBytes [0], "Uni #1");
                        Assert.AreEqual (0x03, UniBytes [1], "Uni #2");
                        Assert.AreEqual (0xA3, UniBytes [2], "Uni #3");
                        Assert.AreEqual (0x03, UniBytes [3], "Uni #4");
                }
        
                [Test]
                public void TestEncodingGetBytes2()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        byte[] UniBytes;
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (true, true); //big-endian
                        UniBytes = UnicodeEnc.GetBytes (Unicode);
                        
                        Assert.AreEqual (0x03, UniBytes [0], "Uni #1");
                        Assert.AreEqual (0xA0, UniBytes [1], "Uni #2");
                        Assert.AreEqual (0x03, UniBytes [2], "Uni #3");
                        Assert.AreEqual (0xA3, UniBytes [3], "Uni #4");
                }

                [Test]
                public void TestEncodingGetBytes3()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        byte[] UniBytes = new byte [4];
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (); //little-endian 
                        int Cnt = UnicodeEnc.GetBytes (Unicode.ToCharArray(), 0, Unicode.Length, UniBytes, 0);
                        
                        Assert.AreEqual (4, Cnt, "Uni #1");
                        Assert.AreEqual (0xA0, UniBytes [0], "Uni #2");
                        Assert.AreEqual (0x03, UniBytes [1], "Uni #3");
                        Assert.AreEqual (0xA3, UniBytes [2], "Uni #4");
                        Assert.AreEqual (0x03, UniBytes [3], "Uni #5");
                }
        
                [Test]
                public void TestEncodingDecodingGetBytes1()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (); //little-endian 
                        //Encode the unicode string.
                        byte[] UniBytes = UnicodeEnc.GetBytes (Unicode);
                        //Decode the bytes to a unicode char array.
                        char[] UniChars = UnicodeEnc.GetChars (UniBytes);
                        string Result = new string(UniChars);
                        
                        Assert.AreEqual (Unicode, Result, "Uni #1");
                }

                [Test]
                public void TestEncodingDecodingGetBytes2()
                {
                        //pi and sigma in unicode
                        string Unicode = "\u03a0\u03a3";
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (true, true); //big-endian 
                        //Encode the unicode string.
                        byte[] UniBytes = UnicodeEnc.GetBytes (Unicode);
                        //Decode the bytes to a unicode char array.
                        char[] UniChars = UnicodeEnc.GetChars (UniBytes);
                        string Result = new string(UniChars);
                        
                        Assert.AreEqual (Unicode, Result, "Uni #1");
                }

		[Test]
		public void TestEncodingGetCharCount ()
		{
			byte[] b = new byte[] {255, 254, 115, 0, 104, 0, 105, 0};
			UnicodeEncoding encoding = new UnicodeEncoding ();

			Assert.AreEqual (3, encoding.GetCharCount (b, 2, b.Length - 2), 
							 "GetCharCount #1");
		}

	
                
                [Test]
                public void TestPreamble1()
                {
                        //litle-endian with byte order mark.
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (false, true); 
                        byte[] PreAmble = UnicodeEnc.GetPreamble();

                        Assert.AreEqual (0xFF, PreAmble [0], "Uni #1");
                        Assert.AreEqual (0xFE, PreAmble [1], "Uni #2");
                }

                [Test]
                public void TestPreamble2()
                {
                        //big-endian with byte order mark.
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (true, true); 
                        byte[] PreAmble = UnicodeEnc.GetPreamble();

                        Assert.AreEqual (0xFE, PreAmble [0], "Uni #1");
                        Assert.AreEqual (0xFF, PreAmble [1], "Uni #2");
                }

                [Test]
                public void TestPreamble3()
                {
                        //little-endian without byte order mark.
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding (false, false); 
                        byte[] PreAmble = UnicodeEnc.GetPreamble();

                        Assert.AreEqual (0, PreAmble.Length, "Uni #1");
                }
                
                [Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
                public void TestMaxCharCount()
                {
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding ();
#if NET_2_0
                        // where is this extra 1 coming from?
                        Assert.AreEqual (26, UnicodeEnc.GetMaxCharCount(50), "UTF #1");
                        Assert.AreEqual (27, UnicodeEnc.GetMaxCharCount(51), "UTF #2");
#else
                        Assert.AreEqual (25, UnicodeEnc.GetMaxCharCount(50), "UTF #1");
#endif
                }
        
                [Test]
#if NET_2_0
		[Category ("NotWorking")]
#endif
                public void TestMaxByteCount()
                {
                        UnicodeEncoding UnicodeEnc = new UnicodeEncoding ();
#if NET_2_0
                        // is this extra 2 BOM?
                        Assert.AreEqual (102, UnicodeEnc.GetMaxByteCount(50), "UTF #1");
#else
                        Assert.AreEqual (100, UnicodeEnc.GetMaxByteCount(50), "UTF #1");
#endif
                }

		[Test]
		public void ZeroLengthArrays ()
		{
			UnicodeEncoding encoding = new UnicodeEncoding ();
			encoding.GetCharCount (new byte [0]);
			encoding.GetChars (new byte [0]);
			encoding.GetCharCount (new byte [0], 0, 0);
			encoding.GetChars (new byte [0], 0, 0);
			encoding.GetChars (new byte [0], 0, 0, new char [0], 0);
			encoding.GetByteCount (new char [0]);
			encoding.GetBytes (new char [0]);
			encoding.GetByteCount (new char [0], 0, 0);
			encoding.GetBytes (new char [0], 0, 0);
			encoding.GetBytes (new char [0], 0, 0, new byte [0], 0);
			encoding.GetByteCount ("");
			encoding.GetBytes ("");
		}

		[Test]
		public void ByteOrderMark ()
		{
			string littleEndianString = "\ufeff\u0042\u004f\u004d";
			string bigEndianString = "\ufffe\u4200\u4f00\u4d00";
			byte [] littleEndianBytes = new byte [] {0xff, 0xfe, 0x42, 0x00, 0x4f, 0x00, 0x4d, 0x00};
			byte [] bigEndianBytes = new byte [] {0xfe, 0xff, 0x00, 0x42, 0x00, 0x4f, 0x00, 0x4d};
			UnicodeEncoding encoding;
			
			encoding = new UnicodeEncoding (false, true);
			Assert.AreEqual (BitConverter.ToString(encoding.GetBytes (littleEndianString)), BitConverter.ToString(littleEndianBytes), "BOM #1");
			Assert.AreEqual (BitConverter.ToString(encoding.GetBytes (bigEndianString)), BitConverter.ToString(bigEndianBytes), "BOM #2");
			Assert.AreEqual (encoding.GetString (littleEndianBytes), littleEndianString, "BOM #3");
			Assert.AreEqual (encoding.GetString (bigEndianBytes), bigEndianString, "BOM #4");

			encoding = new UnicodeEncoding (true, true);
			Assert.AreEqual (BitConverter.ToString(encoding.GetBytes (littleEndianString)), BitConverter.ToString(bigEndianBytes), "BOM #5");
			Assert.AreEqual (BitConverter.ToString(encoding.GetBytes (bigEndianString)), BitConverter.ToString(littleEndianBytes), "BOM #6");
			Assert.AreEqual (encoding.GetString (littleEndianBytes), bigEndianString, "BOM #7");
			Assert.AreEqual (encoding.GetString (bigEndianBytes), littleEndianString, "BOM #8");
		}

#if !DOT42
		[Test]
		[Category ("NotWorking")]
		public void GetString_Odd_Count_0 ()
		{
			byte [] array = new byte [3];
			string s = Encoding.Unicode.GetString (array, 0, 3);
			Assert.AreEqual (0, (int) s [0], "0");

			Assert.AreEqual (2, s.Length, "Length");
			Assert.AreEqual (65533, (int) s [1], "1");
		}

		[Test]
		[Category ("NotWorking")]
		public void GetString_Odd_Count_ff ()
		{
			byte [] array = new byte [3] { 0xff, 0xff, 0xff };
			string s = Encoding.Unicode.GetString (array, 0, 3);
			Assert.AreEqual (65535, (int) s [0], "0");

			Assert.AreEqual (2, s.Length, "Length");
			Assert.AreEqual (65533, (int) s [1], "1");
		}
#endif
	}
}

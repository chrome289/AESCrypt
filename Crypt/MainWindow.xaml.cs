using System;
using System.IO;
using System.Security.Cryptography;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Crypt
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //cry();
        }
        public void cry()
        {
            try
            {

                //string original = "d:\\wallpapers\\batman_arkham_origins_12-wallpaper-1366x768.jpg";
                byte[] imageArray = File.ReadAllBytes("d:\\Videos\\Battlefield 4 gameplay.avi");
                //string s = System.Text.Encoding.ASCII.GetString(imageArray, 0, imageArray.Length);
                //s = System.BitConverter.ToString(imageArray);
                //l1.Text=s;
                
                    // Create a new instance of the RijndaelManaged 
                    // class.  This generates a new key and initialization  
                    // vector (IV). 
                    using (RijndaelManaged myRijndael = new RijndaelManaged())
                    {

                        myRijndael.GenerateKey();
                        myRijndael.GenerateIV();
                        // Encrypt the string to an array of bytes. 
                        byte[] encrypted = EncryptStringToBytes(imageArray, myRijndael.Key, myRijndael.IV);
                        //string en = "d:\\en.avi";
                        //File.WriteAllBytes(en, encrypted);
                        imageArray.SetValue(null,0,imageArray.Length-1);
                        GC.Collect();
                        // Decrypt the bytes to a string. 
                        //string roundtrip = DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);
                        byte[] roundtrip = DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);
                        //Display the original data and the decrypted data.
                        //i1.Source=new BitmapImage(new Uri(@"d:\\0.png"));
                        //l2.Text = roundtrip;
                        //byte[] bytes = new byte[roundtrip.Length];
                        //System.Buffer.BlockCopy(roundtrip.ToCharArray(), 0, bytes, 0, bytes.Length);
                        string loc = "d:\\file.avi";
                        File.WriteAllBytes(loc, roundtrip);

                        for (int x = 0; x < myRijndael.Key.Length; x++)
                            lb1.Text = lb1.Text + myRijndael.Key[x];
                        for (int x = 0; x < myRijndael.IV.Length; x++)
                            lb2.Text = lb2.Text + myRijndael.IV[x];
                       // for (int x = 0; x < roundtrip.Length; x++)
                         //   l2.Text = l2.Text + roundtrip[x];
                        MessageBox.Show("done ");
                        //s = System.Text.Encoding.ASCII.GetString(encrypted, 0, encrypted.Length);
                        //s = System.BitConverter.ToString(encrypted);
                        //l1.Text = roundtrip;
                        //i2.Source = new BitmapImage(new Uri(@"d:\\file.jpg"));
                    }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
        }
        static byte[] EncryptStringToBytes(byte[] message, byte[] Key, byte[] IV)
        {
             //Check arguments. 
            if (message == null || message.Length <= 0)
                throw new ArgumentNullException("message");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter encrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            csEncrypt.Write(message, 0, message.Length);
                            csEncrypt.FlushFinalBlock();
                            return msEncrypt.ToArray();
                        }
                        //encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream. 
            //return encrypted;

        }

        static byte[] DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold 
            // the decrypted text. 
            string plaintext = null;

            // Create an RijndaelManaged object 
            //byte[] decrypted;
            // with the specified key and IV. 
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption. 
                using (var stream = new MemoryStream())
                //using (var decryptor = alg.CreateDecryptor())
                using (var encrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
                {
                    encrypt.Write(cipherText, 0, cipherText.Length);
                    encrypt.FlushFinalBlock();
                    return stream.ToArray();
                }

            }

            //return plaintext;

        }

        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            cry();
        }

        private void bt2_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSAExperiment
{
    class Program
    {
        //Shitty random number generator
        static Random randGen = new Random();

        //Test it!
        static void Main(string[] args)
        {
            for (int i = 0; i < 100000; i++)
            {
                int bitDepth = 32;
                uint maxPrime = (uint)(Math.Pow(2, bitDepth)-1);
                uint firstPrimeMax= (uint)Math.Sqrt(maxPrime);
                uint p = getPrime(firstPrimeMax);
                uint secondPrimeMax= (uint)Math.Floor((double)maxPrime / (double)p);
                uint q = getPrime(secondPrimeMax);
                uint n = p * q;
                uint eu = (p - 1) * (q - 1);
                uint e = getRandomCoprime(eu);
                if (e == 0) Console.WriteLine("Failed to Generate Coprime");
                uint d0 = eGCD(eu, e);
                uint d = eu + d0;
                string testM = "Kevin is awesome!";
                List<uint> encryptedM = encrypt(testM, e, n);
                string decryptedM = decrypt(encryptedM, d, n,p,q);
                if (testM != decryptedM) Console.WriteLine("Process Failed");
            }
            Console.WriteLine("End");
            Console.ReadLine();
        }

        //Decrypt the message using the private key (from http://en.wikipedia.org/wiki/RSA_(algorithm)#Decryption )
        static string decrypt(List<uint> message, uint d, uint n, uint p, uint q)
        {
            string decryptedMessage = "";
            uint dp = d%(p-1);
            uint dq = d%(q-1);
            uint qinv = eGCD(q,p);
            for (int i = 0; i < message.Count; i++)
            {
                uint m = fast_modular_pow(message[i], d, n,p,q,dp,dq,qinv);
                byte[] bytes = BitConverter.GetBytes(m);
                decryptedMessage += (char)bytes[3];
                decryptedMessage += (char)bytes[2];
                decryptedMessage += (char)bytes[1];
                decryptedMessage += (char)bytes[0];
            }
            return decryptedMessage;
        }

        //Encrypt the message using the public key (from http://en.wikipedia.org/wiki/RSA_(algorithm)#Encryption )
        static List<uint> encrypt(string message, uint e, uint n)
        {
            for (int i = 4-message.Length % 4; i > 0; i--)
                message += '\0';
            List<uint> encryptedMessage = new List<uint>();
            for (int i = 0; i < message.Length; i+=4)
            {
                uint m = (uint)(message[i] << 8) + (uint)(message[i + 1]) + (uint)(message[i + 2]) + (uint)(message[i + 3]);
                uint c = modular_pow(m, e, n);
                encryptedMessage.Add((uint)c);
            }
            return encryptedMessage;
        }

        //From http://en.wikipedia.org/wiki/Modular_exponentiation#Memory-efficient_method
        static uint modular_pow(uint b, uint e, uint m)
        {
            uint c = 1;
            for (int e_prime = 1; e_prime <= e; e_prime++)
                c = (c * b) % m;
            return c;
        }

        static uint fast_modular_pow(uint b, uint e, uint m, uint p, uint q, uint dp, uint dq, uint qinv)
        {
            uint m1 = modular_pow(b, dp, p);
            uint m2 = modular_pow(b, dq, q);
            uint h = qinv * (m1 - m2) % p;
            return m2 + h * q;
        }

        //From https://en.wikipedia.org/wiki/Euclidean_algorithm#Implementations
        static uint GCD(uint a0, uint b0)
        {
            uint a = a0;
            uint b = b0;

            while(b != 0)
            {
                uint t = b;
                b = a % t;
                a = t;
            }
            return a;
        }

        //From http://en.wikipedia.org/wiki/Extended_Euclidean_algorithm#Iterative_method_2
        static uint eGCD(uint a0, uint b0)
        {
            uint a = a0;
            uint b = b0;
            uint x = 0, lastx = 1, y = 1, lasty = 0;
            while(b != 0)
            {
                uint quotient = (uint)Math.Floor(((double)a / (double)b));
                uint b1 = b;
                uint a1 = a;
                b = (uint)a1 % (uint)b1;
                a = b1;
                uint x1 = x;
                x = lastx - quotient*x1;
                lastx = x1;
                uint y1 = y;
                y = lasty - quotient*y1;
                lasty = y1;
            }
            return lasty;
        }

        //Made this up based on definition of coprimes ( http://en.wikipedia.org/wiki/Coprime_integers ) in an attempt to generate a random coprime with as little bias as possible
        static uint getRandomCoprime(uint x)
        {
            //Start searching for a coprime from a random number within the limits of the RSA definition ( http://en.wikipedia.org/wiki/RSA_(algorithm)#Key_generation )
            uint startSearchNum = (uint)randGen.Next(1, (int)((x / 2) - 1)) + (uint)randGen.Next(1, (int)((x / 2) - 1));
            //If our guess is coprime, return it
            if (GCD(startSearchNum, x) == 1) return startSearchNum;

            //Search in both directions
            uint n0 = startSearchNum-1, n1 = startSearchNum+1;
            for (uint i = 2; i < x; i++)
            {
                //If the value in either direction is within range and is coprime, flag it as valid
                bool n0_valid = false, n1_valid = false;
                if (!(n0>=x)&&!(n0<=1)&&GCD(n0, x) == 1)
                    n0_valid = true;
                if (!(n1>=x)&&!(n1<=1)&&GCD(n1, x) == 1)
                    n1_valid = true;
                if (n0_valid && n1_valid)
                {
                    //If both are valid at once, pick a random one to return
                    if (randGen.Next(0, 1) == 0)
                        return n0;
                    else
                        return n1;
                }
                //If only one is valid, return it
                else if (n0_valid)
                    return n0;
                else if (n1_valid)
                    return n1;

                //Iterate to the next search values in both directions
                n0 = startSearchNum - i;
                n1 = startSearchNum + i;
            }
            return 0; //Uh oh! We didn't find a coprime! Encryption will not work.
        }

        //Get a random prime below maxVal
        static uint getPrime(uint maxVal)
        {
            uint startSearchNum = (uint)randGen.Next(2, (int)maxVal);
            //If our guess is coprime, return it
            if (isPrime((int)startSearchNum)) return startSearchNum;

            //Search in both directions
            uint n0 = startSearchNum - 1, n1 = startSearchNum + 1;
            for (uint i = 2; i < maxVal; i++)
            {
                //If the value in either direction is within range and is coprime, flag it as valid
                bool n0_valid = false, n1_valid = false;
                if (!(n0 >= maxVal) && !(n0 <= 2) && isPrime((int)n0))
                    n0_valid = true;
                if (!(n1 >= maxVal) && !(n1 <= 2) && isPrime((int)n1))
                    n1_valid = true;
                if (n0_valid && n1_valid)
                {
                    //If both are valid at once, pick a random one to return
                    if (randGen.Next(0, 1) == 0)
                        return n0;
                    else
                        return n1;
                }
                //If only one is valid, return it
                else if (n0_valid)
                    return n0;
                else if (n1_valid)
                    return n1;

                //Iterate to the next search values in both directions
                n0 = startSearchNum - i;
                n1 = startSearchNum + i;
            }
            return 0; //Uh oh! We didn't find a coprime! Encryption will not work.
        }

        //Check if a number is prime
        static bool isPrime(int x)
        {
            // Test whether the parameter is a prime number.
            if ((x & 1) == 0)
            {
                if (x == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // Note:
            // ... This version was changed to test the square.
            // ... Original version tested against the square root.
            // ... Also we exclude 1 at the end.
            for (int i = 3; (i * i) <= x; i += 2)
            {
                if ((x % i) == 0)
                {
                    return false;
                }
            }
            return x != 1;
        }
    }
}

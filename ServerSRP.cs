using M9Studio.SecureStream;
using M9Studio.ShadowTalk.Core;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Utilities.Encoders;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace M9Studio.ShadowTalk.Server
{
    public partial class Server
    {
        protected void NewSRP(SecureSessionLogger session, User user)
        {
            // Получаем входной пароль (например, хэш от клиента после login)
            string password = user.Password;

            // Генерация новой соли
            byte[] saltBytes = SRPHelper.GenerateRandomBytes(16);
            string saltHex = Convert.ToHexString(saltBytes);

            // Вычисляем x = H(salt | password)
            byte[] xHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(saltHex + password));
            BigInteger x = new BigInteger(xHash, isUnsigned: true, isBigEndian: true);

            // Вычисляем v = g^x mod N
            BigInteger N = SRPConstants.N;
            BigInteger g = SRPConstants.g;
            BigInteger k = SRPConstants.k;
            BigInteger v = BigInteger.ModPow(g, x, N);

            // Генерация b и вычисление B = (k*v + g^b) mod N
            byte[] bBytes = SRPHelper.GenerateRandomBytes(32);
            BigInteger b = new BigInteger(bBytes, isUnsigned: true, isBigEndian: true);
            BigInteger gb = BigInteger.ModPow(g, b, N);
            BigInteger B = (k * v + gb) % N;
            string BHex = B.ToString("X");

            // Обновляем user-объект
            user.Salt = saltHex;
            user.Verifier = v.ToString("X");

            // Отправка B, salt
            PacketServerToClientChallengeSRP srp = new PacketServerToClientChallengeSRP
            {
                Name = user.Name,
                Id = user.Id,
                Salt = saltHex,
                B = BHex
            };
            session.Send(srp);

            // Получение A и M1
            PacketClientToServerResponseSRP rsp2;
            try
            {
                rsp2 = PacketStruct.Parse<PacketClientToServerResponseSRP>(session.ReceiveJObject());
            }
            catch (Exception ex)
            {
                Disconnect(session);
                return;
            }

            // Проверка A и M1
            BigInteger A = BigInteger.Parse(rsp2.A, NumberStyles.HexNumber);
            BigInteger u = SRPHelper.ComputeU(A, B, N);
            BigInteger S = BigInteger.ModPow((A * BigInteger.ModPow(v, u, N)) % N, b, N);

            byte[] K = SRPHelper.Hash(S);

            string M1_expected = SRPHelper.ComputeM1(A, B, K);
            /*
            Debug.WriteLine($"Server X = {x}");
            Debug.WriteLine("SERVER SALT: " + saltHex);
            Debug.WriteLine($"SERVER A = {rsp2.A}");
            Debug.WriteLine($"SERVER B = {B.ToString("X")}");
            Debug.WriteLine($"SERVER u = {u}");
            Debug.WriteLine($"SERVER S = {S.ToString("X")}");
            Debug.WriteLine($"SERVER K = {Convert.ToHexString(K)}");
            Debug.WriteLine($"SERVER M1_expected = {M1_expected}");
            Debug.WriteLine($"SERVER M1_received = {rsp2.M1}");
            */

            if (rsp2.M1 != M1_expected)
            {
                Disconnect(session);
                return;
            }
            user.RSA = rsp2.RSA;
            user.B = BHex;
            user.b = b.ToString("X");

            // Обновляем базу
            @base.Send("UPDATE users SET salt = ?, verifier = ?, rsa = ?, b1 = ?, b2 = ? WHERE id = ?", user.Salt, user.Verifier, user.RSA, user.B, user.b, user.Id);

            @base.Send("UPDATE messages SET type = ?, text = ? WHERE recipient = ?", (int)PacketServerToClientStatusMessages.CheckType.DELETED, "", user.Id);


            user.Port = rsp2.Port;
            LoginSuccess(session, user);
        }
        protected void CheckSRP(SecureSessionLogger session, PacketClientToServerReconectSRP srp)
        {
            try
            {
                // Разбор токена
                string[] parts = srp.Token.Split(':');
                if (parts.Length != 2)
                    throw new Exception("Invalid token format");

                string userIdStr = parts[0];
                string timestampStr = parts[1];
                if (!int.TryParse(userIdStr, out int userId))
                    throw new Exception("Invalid userId");

                if (sessions.ContainsKey(userId))
                    throw new Exception("User online");

                if (!long.TryParse(timestampStr, out long tokenTimestamp))
                    throw new Exception("Invalid timestamp");

                long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (currentTimestamp - tokenTimestamp > 3600) // 1 час
                    throw new Exception("Token expired");

                // Поиск пользователя
                List<User> users = @base.Users("SELECT * FROM users WHERE id = ?", userId);
                if (users.Count == 0)
                    throw new Exception("User not found");

                User user = users[0];

                // Восстановление параметров SRP
                BigInteger N = SRPConstants.N;
                BigInteger g = SRPConstants.g;
                BigInteger k = SRPConstants.k;

                BigInteger v = BigInteger.Parse(user.Verifier, NumberStyles.HexNumber);
                BigInteger A = BigInteger.Parse(srp.A, NumberStyles.HexNumber);
                BigInteger B = BigInteger.Parse(user.B, NumberStyles.HexNumber);
                BigInteger b = BigInteger.Parse(user.b, NumberStyles.HexNumber);

                BigInteger u = SRPHelper.ComputeU(A, B, N);
                BigInteger S = BigInteger.ModPow((A * BigInteger.ModPow(v, u, N)) % N, b, N);
                byte[] K = SRPHelper.Hash(S);

                string expectedHmac = SRPHelper.ComputeHMAC(K, srp.Token);



                Debug.WriteLine("Server: ");
                Debug.WriteLine($"Verifier {user.Verifier}");
                Debug.WriteLine($"A {A.ToString("X")}");
                Debug.WriteLine($"B {B}");
                Debug.WriteLine($"u {u}");
                Debug.WriteLine($"S {S}");
                Debug.WriteLine($"k {k}");
                Debug.WriteLine($"K {Convert.ToHexString(K)}");
                Debug.WriteLine($"expectedHmac {expectedHmac}");



                if (!expectedHmac.Equals(srp.HMAC, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Invalid HMAC");

                user.Port = srp.Port;
                LoginSuccess(session, user);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"[Reconnect Fail] {ex.Message}");
                Debug.WriteLine(ex.Message);
                session.Send(new PacketServerToClientLoginError());
                Disconnect(session);
            }
        }
    }
}

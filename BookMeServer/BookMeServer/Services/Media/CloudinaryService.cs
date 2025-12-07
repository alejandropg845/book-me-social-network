using BookMeServer.Models;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;

namespace BookMeServer.Services.Media
{
    public static class CloudinaryService
    {
        public async static Task DeleteImageFromCloudinary(this string publicId, string key,
            string secret)
        {

            var account = new Account
            {
                ApiKey = key,
                ApiSecret = secret,
                Cloud = "dyihpj2hw"
            };

            var cloudinary = new Cloudinary(account);

            var deletionParams = new DeletionParams(publicId);

            await cloudinary.DestroyAsync(deletionParams);

        }
    }
}

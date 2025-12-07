using Azure.AI.ContentSafety;
using Azure;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace BookMeServer.Services
{
    public static class ContentSafetyService
    {

        public static async Task<(bool IsExplicitImage, bool IsExplicitDescription)> VerifyContent(this string imageUrl, string cs_endpoint,
            string cs_key, string description)
        {
            var http = new HttpClient();

            byte[] imageBytes = await http.GetByteArrayAsync(imageUrl);


            ContentSafetyClient contentSafetyClient =
                new ContentSafetyClient
                (
                    new Uri(cs_endpoint),
                    new AzureKeyCredential(cs_key)
                );

            BlocklistClient blocklistClient =
            new BlocklistClient
                (
                    new Uri(cs_endpoint),
                    new AzureKeyCredential(cs_key)
                );


            BinaryData imageData = BinaryData.FromBytes(imageBytes);
            

            var imageAnalysisTask = contentSafetyClient.AnalyzeImageAsync(imageData);

            var descriptionAnalysisTask = contentSafetyClient.AnalyzeTextAsync(description);

            await Task.WhenAll(imageAnalysisTask, descriptionAnalysisTask).ConfigureAwait(false);

            return new
                (
                item1: ImageAnalysisResult(imageAnalysisTask.Result.Value.CategoriesAnalysis),
                item2: DescriptionAnalysisResult(
                    descriptionAnalysisTask.Result.Value.CategoriesAnalysis, 
                    description)
                );

        }

        public static bool ImageAnalysisResult(IReadOnlyList<ImageCategoriesAnalysis> r)
        {

            if (r.FirstOrDefault(a => a.Category == ImageCategory.Violence)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == ImageCategory.SelfHarm)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == ImageCategory.Hate)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == ImageCategory.Sexual)?.Severity > 2)

                return true;

            return false;

        }

        public static bool DescriptionAnalysisResult(IReadOnlyList<TextCategoriesAnalysis> r, string description)
        {

            var request = new AnalyzeTextOptions(description);

            if (r.FirstOrDefault(a => a.Category == TextCategory.Violence)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == TextCategory.SelfHarm)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == TextCategory.Hate)?.Severity != 0
                || r.FirstOrDefault(a => a.Category == TextCategory.Sexual)?.Severity != 0)

                return true;

            return false;
        }


    }
}

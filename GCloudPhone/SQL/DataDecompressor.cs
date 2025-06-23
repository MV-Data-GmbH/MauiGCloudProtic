using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Text.Json;
namespace GCloudPhone
{
    public class DataDecompressor
    {
        public async Task<(List<Categories>, List<Groups>, List<Pictures>, List<Prices>, List<Prices_Type>,
            List<Products>, List<Products_SD>, List<SDGroups>, List<SDPages>, List<Sidedishes>, List<VAT>, List<Stores>, List<StaticPicture>)>
            DecompressAndDeserializeAllData(byte[] compressedData)
        {
            using (var stream = new MemoryStream(compressedData))
            using (var reader = new BinaryReader(stream))
            {
                var categories = await SafeDecompressAndDeserializeData<Categories>(reader, "Categories");
                var groups = await SafeDecompressAndDeserializeData<Groups>(reader, "Groups");
                var pictures = await SafeDecompressAndDeserializeData<Pictures>(reader, "Pictures");
                var prices = await SafeDecompressAndDeserializeData<Prices>(reader, "Prices");
                var priceTypes = await SafeDecompressAndDeserializeData<Prices_Type>(reader, "PriceTypes");
                var products = await SafeDecompressAndDeserializeData<Products>(reader, "Products");
                var productSDs = await SafeDecompressAndDeserializeData<Products_SD>(reader, "ProductSDs");
                var sdGroups = await SafeDecompressAndDeserializeData<SDGroups>(reader, "SDGroups");
                var sdPages = await SafeDecompressAndDeserializeData<SDPages>(reader, "SDPages");
                var sidedishes = await SafeDecompressAndDeserializeData<Sidedishes>(reader, "Sidedishes");
                var vats = await SafeDecompressAndDeserializeData<VAT>(reader, "VATs");
                var stores = await SafeDecompressAndDeserializeData<Stores>(reader, "Stores");
                var staticPictures = await SafeDecompressAndDeserializeData<StaticPicture>(reader, "StaticPictures");
                return (categories, groups, pictures, prices, priceTypes, products,
                    productSDs, sdGroups, sdPages, sidedishes, vats, stores, staticPictures);
            }
        }
        private async Task<List<T>> SafeDecompressAndDeserializeData<T>(BinaryReader reader, string typeName)
        {
            try
            {
                var length = reader.ReadInt32();
                var buffer = new byte[length];
                await reader.BaseStream.ReadAsync(buffer, 0, length);
                using (var compressedStream = new MemoryStream(buffer))
                using (var decompressedStream = new MemoryStream())
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    await gzipStream.CopyToAsync(decompressedStream);
                    decompressedStream.Position = 0;
                    var json = Encoding.UTF8.GetString(decompressedStream.ToArray());
                    return JsonSerializer.Deserialize<List<T>>(json);

                }
            }
            catch (Exception)
            {
                return new List<T>();
            }
        }
    }
}
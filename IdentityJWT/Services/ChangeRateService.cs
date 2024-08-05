using PinPinServer.Models.DTO;
using System.Text.Json;

namespace PinPinServer.Services
{
    public class ChangeRateService
    {
        private readonly string _apikey;
        private readonly HttpClient _httpClient;

        public ChangeRateService(string? apiKey, HttpClient httpClient)
        {
            _apikey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<ChangeRateDTO>> GetChangeRate(string code, List<CostCategoryDTO> dtos)
        {
            string ChangeRateAPI = $"v6/{_apikey}/latest/{code}";
            HttpResponseMessage response = await _httpClient.GetAsync(ChangeRateAPI);
            if (response.IsSuccessStatusCode)
            {
                return ProcessRateData(await response.Content.ReadAsStringAsync(), dtos);
            }
            //使用免費的api回傳
            return [];
        }

        private List<ChangeRateDTO> ProcessRateData(string data, List<CostCategoryDTO> dtos)
        {
            List<ChangeRateDTO> changeRateDTOs = new List<ChangeRateDTO>();
            JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            JsonElement code = root.GetProperty("conversion_rates");
            foreach (CostCategoryDTO dto in dtos)
            {
                if (String.IsNullOrEmpty(dto.Code)) return [];
                if (!code.GetProperty($"{dto.Code}").TryGetDecimal(out decimal rate)) return [];

                changeRateDTOs.Add(new ChangeRateDTO
                {
                    Name = dto.Name,
                    Rate = rate,
                    Code = dto.Code,
                    Icon = dto.Icon,
                    Id = dto.Id,
                });
            }
            return changeRateDTOs;
        }
    }
}
﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WeatherDB.Models;
using WeatherDB.Services;

namespace WeatherDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        private readonly WeatherService _apiService;

        public WeatherController(WeatherService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Get() 
        {
            try
            {
                var retorno = _apiService.Get();
                return Ok(Utf8Json.JsonSerializer.ToJsonString(retorno));
            }
            catch (TimeoutException e)
            {
                return NotFound($"Não foi possível conectar ao servidor do banco de dados MongoDb, verifique se o serviço está ativo e estável no servidor. - Erro: {e.Message}");
            }
            catch (Exception e)
            {
                return NotFound(e);
            }
        } 

        [HttpGet("{cityCode:length(24)}")]
        public IActionResult Get([FromRoute] string cityCode)
        {
            var api = _apiService.GetWeathersCity(cityCode);

            if (api == null)
            {
                return NotFound();
            }

            return Ok(Utf8Json.JsonSerializer.ToJsonString(api));
        }

        [HttpPost]
        [Route("search/{city}")]
        public async Task<IActionResult> SerchWeatherCityApi([FromRoute] string city)
        {
            var _apiWeather = new ConnectWeatherApiService(new RestClient());
            var response = await _apiWeather.ConsumeWeatherApiService(new string[1] { city });

            return ProcessWeathersList(response);
        }

        [HttpPost]
        [Route("search_cities/{cities}")]
        public async Task<IActionResult> SerchWeatherCitiesApi([FromRoute] string cities)
        {
            string[] citiesArr = GetCitiesSplitedWithSeparator(cities);
            var _apiService = new ConnectWeatherApiService(new RestClient());

            var response = await _apiService.ConsumeWeatherApiService(citiesArr);
            return ProcessWeathersList(response);
        }

        private IActionResult ProcessWeathersList(WeathersList response)
        {
            if (response == null || response.List == null)
                return NotFound(JsonConvert.SerializeObject(response));

            if ("Success".Equals(response.MessageResponse))
            {
                _apiService.Create(response);
                response.MessageResponse += $"weathers created in db from weathers returned from api";

                return Ok(JsonConvert.SerializeObject(response));
            }
            else
                return BadRequest(JsonConvert.SerializeObject(response));
        }

        private static string[] GetCitiesSplitedWithSeparator(string cities)
        {
            var citiesArr = cities.Split(',');
            if (citiesArr.Length == 0)
                citiesArr = cities.Split(';');

            return citiesArr;
        }
    }
}
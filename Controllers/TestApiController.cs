﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MicroServiceTestApi.Models;
using MicroServiceTestApi.Services;
using System.Threading.Tasks;

namespace MicroServiceTestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestApiController : ControllerBase
    {
        private readonly TestApiService _apiService;

        public TestApiController(TestApiService apiService)
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

        [HttpGet("{id:length(24)}")]
        public IActionResult Get([FromRoute] string id)
        {
            var api = _apiService.Get(id);

            if (api == null)
            {
                return NotFound();
            }

            return Ok(Utf8Json.JsonSerializer.ToJsonString(api));
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Create([FromRoute] string id, [FromBody] TestApi api)
        {
            _apiService.Create(api);

            var retorno = CreatedAtRoute("GetTestApi", new { id = api.Id.ToString() }, api);
            return Ok("{ mensagem: Item cadastrado com sucesso. }");
        }

        [HttpPost("{id:length(24)}")]
        public IActionResult Update([FromRoute] string id, [FromBody] TestApi apiIn)
        {
            var api = _apiService.Get(id);

            if (api == null)
            {
                return NotFound();
            }

            _apiService.Update(id, apiIn);
            return Ok("{ mensagem: Item atualizado com sucesso. }");
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete([FromRoute] string id)
        {
            var api = _apiService.Get(id);

            if (api == null)
            {
                return NotFound();
            }

            _apiService.Remove(api.Id);

            return Ok("{ mensagem: Item removido com sucesso. }");
        }
    }
}
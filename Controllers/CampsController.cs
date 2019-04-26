using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        //Accept a query string for includeTalks : /api/camps/?includeTalks=true
        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            try
            {
                var results = await this.repository.GetAllCampsAsync(includeTalks);

                //As in the method definition we indicated the retuning typy as CampModel[] array, it will return Ok automatically, so we can return the result, without the Ok
                //return Ok(results);
                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

        }

        //Adding a route value binded to the parameter in the method. string is the default type : /api/camps/ATL2018
        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await repository.GetCampAsync(moniker);
                if (result == null) return NotFound();

                return mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        //Include search as an extension to the URI and accept query strings as parameters : /api/camps/search?theDate=2018-10-18&includeTalks=true
        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await repository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any()) return NotFound();

                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var MonikerExist = await repository.GetCampAsync(model.Moniker);

                if (MonikerExist != null)
                {
                    return BadRequest("Bad request : Moniker already exist");
                }

                var location = linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                //Create a new Camp model instance. Need to add ReverseMap in the profiler, to make it work the other way
                var camp = mapper.Map<Camp>(model);
                repository.Add(camp);
                if (await repository.SaveChangesAsync())
                {
                    //Using hard coded values for the returning URI created resource
                    //return Created($"/api/camps/{camp.Moniker}", mapper.Map<CampModel>(camp));

                    //With dotnet core 2.1 and above we can use LinkGenerator class to create the new link
                    return Created(location, mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal error failure : {ex.Message}");
            }

            return BadRequest();
        }


        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                //Get existing camp from the repository
                var oldCamp = await repository.GetCampAsync(moniker);
                if (oldCamp == null) return BadRequest($"Could not find camp with provied moniker : {moniker}");

                //Update model with new values using the mapper
                mapper.Map(model, oldCamp);
                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal error failure : {ex.Message}");
            }
            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                //Get existing camp from the repository
                var oldCamp = await repository.GetCampAsync(moniker);
                if (oldCamp == null) return BadRequest($"Could not find camp with provied moniker : {moniker}");

                repository.Delete(oldCamp);
                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }

            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal error failure : {ex.Message}");
            }
            return BadRequest();
        }

    }
}

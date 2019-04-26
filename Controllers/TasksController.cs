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
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TasksController : ControllerBase
    {
        private readonly ICampRepository repository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;

        public TasksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await repository.GetTalksByMonikerAsync(moniker, true);
                return mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal error failure : {ex.Message}");
            }
        }

        [HttpGet("{talkId:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int talkId)
        {
            try
            {
                var talk = await repository.GetTalkByMonikerAsync(moniker, talkId, true);
                if (talk == null) return BadRequest("Talks was not found for this Camp");
                return mapper.Map<TalkModel>(talk);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal error failure : {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                //validate camp exist
                var camp = await repository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp does not exist");

                if (model.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");

                //create new talk instance from input model parameter
                var talk = mapper.Map<Talk>(model);
                talk.Camp = camp;
                talk.Speaker = speaker;
                repository.Add(talk);
                if (await repository.SaveChangesAsync())
                {
                    //var location = linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });
                    var location = linkGenerator.GetPathByAction(HttpContext,
                        "Get",
                        values: new { moniker, talkId = talk.TalkId });
                    return Created(location, mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal error failure : {ex.Message}");
            }
            return BadRequest();

        }

        [HttpPut]
        public async Task<ActionResult<TalkModel>> Put(string moniker, TalkModel model)
        {
            try
            {

                //Get talk by ID
                var talk = await repository.GetTalkByMonikerAsync(moniker, model.TalkId, true);
                if (talk == null) return NotFound("Talk with provided id was not found");

                //Assign new values
                mapper.Map(model, talk);

                //If speaker is provided, assign it
                if (model.Speaker != null)
                {
                    var speaker = await repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await repository.SaveChangesAsync())
                {
                    return mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to update the talk");
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal error failure : {ex.Message}");
            }
        }

        [HttpDelete("{talkId:int}")]
        public async Task<IActionResult> Delete(string moniker, int talkId)
        {
            try
            {
                //Get talk by ID
                var talk = await repository.GetTalkByMonikerAsync(moniker, talkId);
                if (talk == null) return NotFound("Talk with provided id was not found");

                repository.Delete(talk);
                if (await repository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Internal error failure : {ex.Message}");
            }
            return BadRequest("Couldn't delete the talk");

        }
    }
}

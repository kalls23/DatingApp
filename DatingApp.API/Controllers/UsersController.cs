using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Data;
using DatingApp.API.DTOS;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;


namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[Controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IDatingRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _repo.GetUsers();
            var userToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(users);
            return Ok(userToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _repo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailDTO>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDTO userForUpdateDTO) {
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
                
                var userFromRepo = await _repo.GetUser(id);

                _mapper.Map(userForUpdateDTO, userFromRepo);

                if(await _repo.SaveAll())
                    return NoContent();

                throw new System.Exception($"Updating user {id} failed on save");
            
        
    }
}
}
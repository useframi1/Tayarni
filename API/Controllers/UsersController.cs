using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class UsersController : BaseApiController
{
    private readonly DataContext _context;

    public UsersController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<User>> GetUser(string username)
    {
        return await _context.Users.FindAsync(username);
    }

    [HttpGet("flightHistory")]
    public async Task<ActionResult<IEnumerable<UserPredictionDto>>> GetFlightHistory(string username)
    {
        var user = await _context.Users
                                 .Include(u => u.UserPredictions)
                                     .ThenInclude(up => up.DestAirportNavigation)
                                 .Include(u => u.UserPredictions)
                                     .ThenInclude(up => up.OrgAirportNavigation)
                                 .FirstOrDefaultAsync(u => u.Username == username);

        var history = user.UserPredictions
                          .Select(up => new UserPredictionDto
                          {
                              Date = up.Date,
                              ScheduledArrTime = up.ScheduledArrTime,
                              ScheduledDepTime = up.ScheduledDepTime,
                              DestAirportCode = up.DestAirportNavigation.AirportCode,
                              OrgAirportCode = up.OrgAirportNavigation.AirportCode,
                              IsDelayedPredicted = up.IsDelayedPredicted,
                              IsDelayedActual = up.IsDelayedActual,
                          })
                          .ToList();

        return Ok(history);
    }

    [HttpPut("feedback")]
    public async Task<ActionResult> GiveFeedback(FeedbackDto feedbackDto)
    {
        var userPrediction = await _context.UserPredictions.FindAsync(feedbackDto.Username, feedbackDto.ScheduledDepTime, feedbackDto.Date);

        if (userPrediction == null)
        {
            return NotFound();
        }

        userPrediction.IsDelayedActual = feedbackDto.IsDelayedActual;

        if (await TrainingDataExists(userPrediction))
        {
            return Ok(await _context.SaveChangesAsync() > 0);
        }

        var trainingRow = new Training
        {
            Date = userPrediction.Date,
            ScheduledArrTime = userPrediction.ScheduledArrTime,
            AirlineName = userPrediction.AirlineName,
            TailNum = userPrediction.TailNum,
            OrgAirport = userPrediction.OrgAirport,
            DestAirport = userPrediction.DestAirport,
            Distance = userPrediction.Distance,
            ScheduledDepTime = userPrediction.ScheduledDepTime,
            DepTemperature = userPrediction.DepTemperature,
            DepPrecipitation = userPrediction.DepPrecipitation,
            DepRain = userPrediction.DepRain,
            DepSnowFall = userPrediction.DepSnowFall,
            DepWindDirection = userPrediction.DepWindDirection,
            DepWindSpeed = userPrediction.DepWindSpeed,
            ArrTemperature = userPrediction.ArrTemperature,
            ArrPrecipitation = userPrediction.ArrPrecipitation,
            ArrRain = userPrediction.ArrRain,
            ArrSnowFall = userPrediction.ArrSnowFall,
            ArrWindDirection = userPrediction.ArrWindDirection,
            ArrWindSpeed = userPrediction.ArrWindSpeed,
            IsDelayed = (bool)userPrediction.IsDelayedActual,
        };

        _context.Training.Add(trainingRow);

        return Ok(await _context.SaveChangesAsync() > 0);
    }

    private async Task<bool> TrainingDataExists(UserPrediction userPrediction)
    {
        return await _context.Training.AnyAsync(t => t.AirlineName == userPrediction.AirlineName
                                                    && t.ScheduledDepTime == userPrediction.ScheduledDepTime
                                                    && t.TailNum == userPrediction.TailNum
                                                    && t.Date == userPrediction.Date);
    }
}

using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers;

public class NewFlightController : BaseApiController
{
    private readonly DataContext _context;

    public NewFlightController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("airports")]
    public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirports()
    {
        var airports = await _context.Airports
        .Select(a => new AirportDto { AirportName = a.AirportName, AirportCode = a.AirportCode })
        .ToListAsync();

        return Ok(airports);
    }

    [HttpGet("airlines")]
    public async Task<ActionResult<IEnumerable<AirlineDto>>> GetAirlines()
    {
        var airlines = await _context.Airlines
        .Select(a => new AirlineDto { AirlineName = a.AirlineName, UniqueCarrier = a.UniqueCarrier })
        .ToListAsync();

        return Ok(airlines);
    }

    [HttpGet("tailNumbers")]
    public async Task<ActionResult<IEnumerable<TailNumDto>>> GetTailNumbers()
    {
        var tailNums = await _context.TailNumbers
        .Select(a => new TailNumDto { TailNum = a.TailNum })
        .ToListAsync();

        return Ok(tailNums);
    }

    [HttpPost("addFlight")]
    public async Task<ActionResult<bool>> AddNewFlight(NewFlightDto newFlightDto)
    {
        if (await FlightExists(newFlightDto)) return BadRequest("You already have a flight scheduled at that time");

        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(newFlightDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://localhost:5001/api/model/predict", content);
        var result = await response.Content.ReadAsStringAsync();
        var jsonObject = JsonConvert.DeserializeObject<JObject>(result);

        var newFlight = new UserPrediction
        {
            Username = newFlightDto.Username,
            Date = newFlightDto.Date,
            ScheduledArrTime = newFlightDto.ScheduledArrTime,
            AirlineName = newFlightDto.AirlineName,
            TailNum = newFlightDto.TailNum,
            OrgAirport = newFlightDto.OrgAirport,
            DestAirport = newFlightDto.DestAirport,
            Distance = newFlightDto.Distance,
            ScheduledDepTime = newFlightDto.ScheduledDepTime,
            DepTemperature = jsonObject["DepTemperature"].Values<double>().ElementAt(0),
            DepPrecipitation = jsonObject["DepPrecipitation"].Values<double>().ElementAt(0),
            DepRain = jsonObject["DepRain"].Values<double>().ElementAt(0),
            DepSnowFall = jsonObject["DepSnowFall"].Values<double>().ElementAt(0),
            DepWindDirection = jsonObject["DepWindDirection"].Values<double>().ElementAt(0),
            DepWindSpeed = jsonObject["DepWindSpeed"].Values<double>().ElementAt(0),
            ArrTemperature = jsonObject["ArrTemperature"].Values<double>().ElementAt(0),
            ArrPrecipitation = jsonObject["ArrPrecipitation"].Values<double>().ElementAt(0),
            ArrRain = jsonObject["ArrRain"].Values<double>().ElementAt(0),
            ArrSnowFall = jsonObject["ArrSnowFall"].Values<double>().ElementAt(0),
            ArrWindDirection = jsonObject["ArrWindDirection"].Values<double>().ElementAt(0),
            ArrWindSpeed = jsonObject["ArrWindSpeed"].Values<double>().ElementAt(0),
            IsDelayedPredicted = jsonObject["IsDelayedPredicted"].Values<int>().ElementAt(0) == 1,
            IsDelayedActual = null,
        };

        _context.UserPredictions.Add(newFlight);
        await _context.SaveChangesAsync();
        return Ok(newFlight.IsDelayedPredicted);
    }

    private async Task<bool> FlightExists(NewFlightDto newFlightDto)
    {
        return await _context.UserPredictions.AnyAsync(x => x.Username == newFlightDto.Username && x.Date == newFlightDto.Date && x.ScheduledDepTime == newFlightDto.ScheduledDepTime);
    }

}

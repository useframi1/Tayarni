using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API.Controllers;

public class ModelController : BaseApiController
{
    private readonly DataContext _context;

    public ModelController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("predict")]
    public async Task<ActionResult<string>> Predict(NewFlightDto newFlightDto)
    {
        var airline = await _context.Airlines.FindAsync(newFlightDto.AirlineName);
        var orgAirport = await _context.Airports.FindAsync(newFlightDto.OrgAirport);
        var destAirport = await _context.Airports.FindAsync(newFlightDto.DestAirport);

        var modelPredictionDto = new ModelPredictionDto
        {
            Date = newFlightDto.Date,
            ScheduledArrTime = newFlightDto.ScheduledArrTime,
            ScheduledDepTime = newFlightDto.ScheduledDepTime,
            AirlineName = airline.AirlineName,
            UniqueCarrier = airline.UniqueCarrier,
            OrgAirportCode = orgAirport.AirportCode,
            OrgAirport = orgAirport.AirportName,
            DestAirportCode = destAirport.AirportCode,
            DestAirport = destAirport.AirportName,
            TailNum = newFlightDto.TailNum,
            Distance = newFlightDto.Distance,
        };
        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(modelPredictionDto), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("http://localhost:4000/predict", content);
        var result = await response.Content.ReadAsStringAsync();

        return Ok(result);
    }

    [HttpGet("train")]
    public async Task<ActionResult> Train()
    {
        List<ModelTrainingDto> modelTrainingDtos = await _context.Training
            .Join(_context.Airlines, training => training.AirlineName, airline => airline.AirlineName, (training, airline) => new { training, airline })
            .Join(_context.Airports, result => result.training.OrgAirport, airport => airport.AirportName, (result, orgAirport) => new { result.training, result.airline, orgAirport })
            .Join(_context.Airports, result => result.training.DestAirport, airport => airport.AirportName, (result, destAirport) => new { result.training, result.airline, result.orgAirport, destAirport })
            .Select(item => new ModelTrainingDto
            {
                Date = item.training.Date,
                ScheduledArrTime = item.training.ScheduledArrTime,
                ScheduledDepTime = item.training.ScheduledDepTime,
                AirlineName = item.training.AirlineName,
                UniqueCarrier = item.airline.UniqueCarrier,
                OrgAirportCode = item.orgAirport.AirportCode,
                OrgAirport = item.training.OrgAirport,
                DestAirportCode = item.destAirport.AirportCode,
                DestAirport = item.training.DestAirport,
                TailNum = item.training.TailNum,
                Distance = item.training.Distance,
                DepTemperature = item.training.DepTemperature,
                DepPrecipitation = item.training.DepPrecipitation,
                DepRain = item.training.DepRain,
                DepSnowFall = item.training.DepSnowFall,
                DepWindDirection = item.training.DepWindDirection,
                DepWindSpeed = item.training.DepWindSpeed,
                ArrTemperature = item.training.ArrTemperature,
                ArrPrecipitation = item.training.ArrPrecipitation,
                ArrRain = item.training.ArrRain,
                ArrSnowFall = item.training.ArrSnowFall,
                ArrWindDirection = item.training.ArrWindDirection,
                ArrWindSpeed = item.training.ArrWindSpeed,
                IsDelayed = item.training.IsDelayed,
            }).ToListAsync();

        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(modelTrainingDtos), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("http://localhost:4000/train", content);
        var r = await response.Content.ReadAsStringAsync();

        return r == "1" ? Ok() : BadRequest();
    }
}

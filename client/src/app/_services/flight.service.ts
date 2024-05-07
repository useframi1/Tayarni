import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Airport } from '../_models/airport';
import { map } from 'rxjs';
import { Airline } from '../_models/airline';
import { TailNum } from '../_models/tailNum';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class FlightService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getAirports() {
    return this.http.get<Airport[]>(this.baseUrl + 'newflight/airports').pipe(
      map((response: Airport[]) => {
        return response;
      })
    );
  }

  getAirlines() {
    return this.http.get<Airline[]>(this.baseUrl + 'newflight/airlines').pipe(
      map((response: Airline[]) => {
        return response;
      })
    );
  }

  getTailNums() {
    return this.http
      .get<TailNum[]>(this.baseUrl + 'newflight/tailNumbers')
      .pipe(
        map((response: TailNum[]) => {
          return response;
        })
      );
  }

  addNewFlight(model: any) {
    return this.http
      .post<boolean>(this.baseUrl + 'newflight/addFlight', model)
      .pipe(
        map((response) => {
          return response;
        })
      );
  }
}

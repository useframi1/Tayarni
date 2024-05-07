import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Flight } from '../_models/flight';
import { map } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getFlightHistory(username: string) {
    return this.http
      .get<Flight[]>(this.baseUrl + 'users/flightHistory?username=' + username)
      .pipe(
        map((response: Flight[]) => {
          return response;
        })
      );
  }

  feedback(model: any) {
    return this.http.put<boolean>(this.baseUrl + 'users/feedback', model).pipe(
      map((response) => {
        return response;
      })
    );
  }
}

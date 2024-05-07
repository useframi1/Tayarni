import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FlightService } from '../_services/flight.service';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import * as bootstrap from 'bootstrap';

@Component({
  selector: 'app-new-flight',
  templateUrl: './new-flight.component.html',
  styleUrls: ['./new-flight.component.css'],
})
export class NewFlightComponent implements OnInit {
  model: any = {};
  username: string = '';
  flightForm: FormGroup = new FormGroup({});
  minDepDate: Date = new Date();
  minArrDate: Date = new Date();
  maxDepDate: Date = new Date();
  maxArrDate: Date = new Date();
  minDistance: number = 0;
  airports: any;
  airlines: any;
  tailNums: any;
  delayed: boolean = false;

  constructor(
    private flightService: FlightService,
    private accountService: AccountService,
    private toastr: ToastrService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.getCurrentUsername();
    this.maxDepDate.setDate(this.maxDepDate.getDate() + 14);
    this.maxArrDate.setDate(this.maxArrDate.getDate() + 14);
    if (this.maxDepDate.getMonth() > 5) {
      this.maxDepDate.setMonth(5); // June
      this.maxDepDate.setDate(30); // Last day of June
    }
    if (this.maxArrDate.getMonth() > 5) {
      this.maxArrDate.setMonth(5); // June
      this.maxArrDate.setDate(30); // Last day of June
    }
    this.initializeForm();

    this.flightService.getAirports().subscribe({
      next: (response) => {
        this.airports = response;
        for (let i = 0; i < response.length; i++) {
          this.airports[i] = response[i].airportName;
        }
      },
      error: (error) => console.log(error),
    });
    this.flightService.getAirlines().subscribe({
      next: (response) => {
        this.airlines = response;
        for (let i = 0; i < response.length; i++) {
          this.airlines[i] = response[i].airlineName;
        }
      },
      error: (error) => console.log(error),
    });
    this.flightService.getTailNums().subscribe({
      next: (response) => {
        this.tailNums = response;
        for (let i = 0; i < response.length; i++) {
          this.tailNums[i] = response[i].tailNum;
        }
      },
      error: (error) => console.log(error),
    });
  }

  getCurrentUsername() {
    this.accountService.currentUser$.subscribe((user) => {
      if (user) {
        this.username = user.username;
      }
    });
  }

  initializeForm() {
    this.flightForm = this.fb.group({
      depDate: ['', Validators.required],
      arrDate: ['', Validators.required],
      depTime: ['', [Validators.required, this.validateDepDate()]],
      arrTime: ['', [Validators.required, this.validateArrDate('depTime')]],
      orgAirport: ['', Validators.required],
      destAirport: ['', Validators.required],
      airline: ['', Validators.required],
      tailNum: ['', Validators.required],
      distance: ['', [Validators.required, Validators.min(this.minDistance)]],
    });
    this.flightForm.controls['depDate'].valueChanges.subscribe((depDate) => {
      if (depDate) {
        this.flightForm.controls['arrTime'].updateValueAndValidity();
        this.flightForm.controls['depTime'].updateValueAndValidity();
        this.minArrDate = new Date(this.flightForm.controls['depDate']?.value);
        this.flightForm.controls['arrDate'].setValue(this.minArrDate);
      }
    });
    this.flightForm.controls['arrDate'].valueChanges.subscribe((arrDate) => {
      if (arrDate) {
        this.flightForm.controls['arrTime'].updateValueAndValidity();
      }
    });
  }

  validateDepDate(): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      let currentTime = new Date();
      currentTime = new Date(currentTime.setSeconds(0));
      const depTime = control.value;
      if (!depTime) return null;
      let date = new Date(this.flightForm.controls['depDate'].value);
      date = new Date(date.setMinutes(depTime.slice(3, 5)));
      date = new Date(date.setHours(depTime.slice(0, 2)));
      date = new Date(date.setSeconds(0));

      return date <= currentTime ? { departureBeforeCurrent: true } : null;
    };
  }

  validateArrDate(compareTo: string): ValidatorFn {
    return (control: AbstractControl): { [key: string]: any } | null => {
      const dep = control.parent?.get(compareTo)?.value;
      const arr = control.value;
      if (!this.flightForm.controls['depDate']?.value) return null;
      let depTime = new Date(this.flightForm.controls['depDate']?.value);
      depTime = new Date(depTime.setMinutes(dep.slice(3, 5)));
      depTime = new Date(depTime.setHours(dep.slice(0, 2)));
      depTime = new Date(depTime.setSeconds(0));

      let arrTime = new Date(this.flightForm.controls['arrDate'].value);
      arrTime = new Date(arrTime.setMinutes(arr.slice(3, 5)));
      arrTime = new Date(arrTime.setHours(arr.slice(0, 2)));
      arrTime = new Date(arrTime.setSeconds(0));

      return depTime && arrTime && arrTime < depTime
        ? { arrivalBeforeDeparture: true }
        : null;
    };
  }

  private getDateOnly(depDate: string | undefined) {
    if (!depDate) return;
    let date = new Date(depDate);
    return new Date(
      date.setMinutes(date.getMinutes() - date.getTimezoneOffset())
    )
      .toISOString()
      .slice(0, 10);
  }

  private formatTime(time: string) {
    return time.replace(':', '');
  }

  save() {
    const date = this.getDateOnly(this.flightForm.controls['depDate'].value);
    const depTimeFormatted = this.formatTime(
      this.flightForm.controls['depTime'].value
    );
    const arrTimeFormatted = this.formatTime(
      this.flightForm.controls['arrTime'].value
    );
    const values = {
      ...this.flightForm.value,
      depDate: date,
      depTime: depTimeFormatted,
      arrTime: arrTimeFormatted,
    };
    delete values.arrDate;

    this.model.username = this.username;
    this.model.date = values.depDate;
    this.model.scheduledDepTime = Number(values.depTime);
    this.model.scheduledArrTime = Number(values.arrTime);
    this.model.airlineName = values.airline;
    this.model.orgAirport = values.orgAirport;
    this.model.destAirport = values.destAirport;
    this.model.tailNum = values.tailNum;
    this.model.distance = values.distance;

    this.flightService.addNewFlight(this.model).subscribe({
      next: (response) => {
        if (response != null) {
          this.toastr.success('Flight added successfully');
          this.delayed = response;
        }
      },
      complete: () => {
        const modal = document.getElementById('predictionModal');
        if (modal == null) return;
        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
      },
    });
  }
}

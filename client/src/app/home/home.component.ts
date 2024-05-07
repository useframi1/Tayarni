import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { UserService } from '../_services/user.service';
import { Flight } from '../_models/flight';
import { DatePipe } from '@angular/common';
import { Feedback } from '../_models/feedback';
import { ToastrService } from 'ngx-toastr';
import { BusyService } from '../_services/busy.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  history: Flight[] = [];
  username: string = '';

  constructor(
    public accountService: AccountService,
    private userService: UserService,
    private toastr: ToastrService,
    public busyService: BusyService
  ) {}

  ngOnInit(): void {
    this.getCurrentUsername();
    if (this.username) this.getFlightHistory();
  }

  getCurrentUsername() {
    this.accountService.currentUser$.subscribe((user) => {
      if (user) {
        this.username = user.username;
      }
    });
  }

  getFlightHistory() {
    this.userService.getFlightHistory(this.username).subscribe({
      next: (response) => {
        this.history = response;
      },
      error: (_) => this.toastr.error('Unable to fetch flight history'),
    });
  }

  giveFeedback(flight: Flight) {
    const radioButtons = document.getElementsByName('options-base');
    flight.isDelayedActual = (radioButtons[0] as HTMLInputElement).checked;
    var feedback: Feedback = {
      username: this.username,
      scheduledDepTime: flight.scheduledDepTime,
      date: flight.date,
      isDelayedActual: flight.isDelayedActual,
    };

    this.userService.feedback(feedback).subscribe({
      next: (response) => {
        if (response) this.toastr.success('Thank you for your feedback!');
      },
      error: (_) => this.toastr.error('Unable to save feedback'),
    });
  }

  convertDate(date: Date) {
    let datePipe = new DatePipe('en-US');
    date = new Date(date);
    let formattedDate = datePipe.transform(date, 'MMMM d yyyy');
    let day = date.getDate();
    let formattedDay = this.getOrdinalIndicator(day);
    if (formattedDate == null) return '';
    let formattedDateWithOrdinal = formattedDate.replace(
      ` ${day} `,
      ` ${formattedDay} `
    );
    return formattedDateWithOrdinal;
  }

  getOrdinalIndicator(day: number): string {
    if (day > 3 && day < 21) return day + 'th';
    switch (day % 10) {
      case 1:
        return day + 'st';
      case 2:
        return day + 'nd';
      case 3:
        return day + 'rd';
      default:
        return day + 'th';
    }
  }

  convertTime(time: number) {
    let timeStr = time.toString();

    while (timeStr.length < 4) {
      timeStr = '0' + timeStr;
    }

    let hours = parseInt(timeStr.substring(0, 2));
    let minutes = timeStr.substring(2);

    let period = hours < 12 ? 'AM' : 'PM';

    hours = hours % 12;
    if (hours === 0) {
      hours = 12;
    }

    return `${hours}:${minutes} ${period}`;
  }

  showFeedback(date: Date, isDelayedActual: boolean | null) {
    date = new Date(date);
    date.setHours(0, 0, 0, 0);
    const now = new Date(Date.now());
    return isDelayedActual == null && date < now;
  }
}

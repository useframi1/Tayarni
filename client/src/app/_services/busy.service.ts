import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class BusyService {
  busyRequestCount = 0;
  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();

  constructor(private spinnerService: NgxSpinnerService) {}

  busy() {
    this.busyRequestCount++;
    this.spinnerService.show(undefined, {
      type: 'ball-clip-rotate',
      bdColor: 'rgba(0,0,0,0.5)',
      color: 'var(--bs-light)',
      size: 'medium',
    });
    this.loadingSubject.next(true);
  }

  idle() {
    this.busyRequestCount--;
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide();
      this.loadingSubject.next(false);
    }
  }
}

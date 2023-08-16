import { Component, OnInit } from '@angular/core';
import { EventService } from '@services/event/event.service';
import { TimerBarEvent } from '@models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-timer-bar',
  templateUrl: './timer-bar.component.html',
  styleUrls: ['./timer-bar.component.sass']
})
export class TimerBarComponent implements OnInit {

  showTimerBar: boolean = true;
  title: string = '';
  timeToHold: number = 0;
  interval?: number;
  percentage: number = 0;
  event$?: Subscription;

  constructor(
    private _event: EventService,
  ) { }

  ngOnInit(): void {
    this.event$ = this._event.onEvent<TimerBarEvent>('fivepd:ui:view:timerBar').subscribe(res => this.onEvent(res));
  }

  ngOnDestroy(): void {
    this.event$?.unsubscribe();
  }

  onEvent(res: TimerBarEvent): void {
    this.showTimerBar = res.Show;

    if (!res.Show) {
      this.title = '';
      this.timeToHold = 0;
      this.percentage = 0;
      clearInterval(this.interval);
      return;
    }

    this.title = res.Title;
    this.timeToHold = res.TimeToHold;

    console.log(this.title)
    console.log(this.timeToHold)

    const addToPercentage = 1000 / this.timeToHold;
    this.interval = setInterval(() => {
      this.percentage += addToPercentage;
    }, 1);
  }

}

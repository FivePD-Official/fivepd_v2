import { Component, OnDestroy, OnInit } from '@angular/core';
import { EventService } from '@services/event/event.service';
import { TimerBarEvent, TimerBarProgressEvent } from '@models';
import { Subscription } from 'rxjs';
import { NgStyle } from '@angular/common';

@Component({
  selector: 'app-timer-bar',
  templateUrl: './timer-bar.component.html',
  standalone: true,
  imports: [NgStyle]
})
export class TimerBarComponent implements OnInit, OnDestroy {

  showTimerBar: boolean = true;
  title: string = '';
  percentage: number = 0;
  private timerBarEventSubscription?: Subscription;
  private timerBarProgressSubscription?: Subscription;

  constructor(
    private _event: EventService,
  ) {
  }

  ngOnInit(): void {
    this.timerBarEventSubscription = this._event.onEvent<TimerBarEvent>('fivepd:ui:view:timer_bar').subscribe(res => this.onTimerBarEvent(res));
    this.timerBarProgressSubscription = this._event.onEvent<TimerBarProgressEvent>('fivepd:ui:view:timer_bar_progress').subscribe(res => this.onTimerBarProgress(res));
  }

  ngOnDestroy(): void {
    this.timerBarEventSubscription?.unsubscribe();
    this.timerBarProgressSubscription?.unsubscribe();
  }

  onTimerBarEvent(res: TimerBarEvent): void {
    this.showTimerBar = res.Show;

    if (!res.Show) {
      this.title = '';
      this.percentage = 100;
      return;
    }

    this.title = res.Title;
    this.percentage = 0;
  }

  onTimerBarProgress(res: { CurrentProgress: number, TotalDuration: number }): void {
    if (res.TotalDuration === 0) {
      this.percentage = 0;
      return;
    }

    this.percentage = (res.CurrentProgress / res.TotalDuration) * 100;

    // Ensure the bar reaches 100% when the timer is logically complete
    if (this.percentage >= 100) {
      this.percentage = 100;
      // Do NOT hide here. Let onTimerBarEvent handle the hide signal.
      // this.showTimerBar = false; // Removed
    } else {
      //this.showTimerBar = true; // Ensure bar is visible during progress
    }
  }

}

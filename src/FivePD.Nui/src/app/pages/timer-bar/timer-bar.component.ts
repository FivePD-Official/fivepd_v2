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
  private hideTimeout: NodeJS.Timeout | undefined;
  private _timerStartTime: number = 0;
  private _currentTotalDuration: number = 0;
  private _lastKnownPercentage: number = 0;

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
    if (this.hideTimeout) {
      clearTimeout(this.hideTimeout);
    }
  }

  onTimerBarEvent(res: TimerBarEvent): void {
    if (this.hideTimeout) {
      clearTimeout(this.hideTimeout);
    }

    if (!res.Show) {
      if (this._lastKnownPercentage < 100) {
        this.title = '';
        this.showTimerBar = false;
        this.percentage = 0;
        return;
      }

      this.percentage = 100;

      const currentTime = Date.now();
      const expectedEndTime = this._timerStartTime + this._currentTotalDuration;
      let delay = expectedEndTime - currentTime;

      delay = Math.max(0, delay);

      this.hideTimeout = setTimeout(() => {
        this.title = '';
        this.showTimerBar = false;
      }, delay * 10);
      return;
    }

    this.showTimerBar = true;
    this.title = res.Title;
    this.percentage = 0;
    this._timerStartTime = Date.now();
  }

  onTimerBarProgress(res: { CurrentProgress: number, TotalDuration: number }): void {
    if (res.TotalDuration === 0) {
      this.percentage = 0;
      this._lastKnownPercentage = 0;
      return;
    }

    this._currentTotalDuration = res.TotalDuration;
    this.percentage = (res.CurrentProgress / res.TotalDuration) * 100;
    this._lastKnownPercentage = this.percentage;
  }

}

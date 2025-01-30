import { inject, Injectable } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { OnAudioPlaySound, OnAudioSpeak } from "@models";
import { Howl } from 'howler';
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class AudioService {

  private _listening = false;
  private readonly _eventService = inject(EventService);

  private _audioVolume = 0;

  constructor(private readonly _store: Store<GlobalState>) {
    _store.select('user', 'details').subscribe(user => {
      this._audioVolume = user?.User?.AudioVolume ?? 0;
    });
  }

  listenToEvents(): void {
    if (this._listening) {
      return;
    }

    this._listening = true;

    this._eventService.onEvent<OnAudioSpeak>('fivepd:audio:playSound').subscribe(e => this.playSound(e.Sound));
    this._eventService.onEvent<OnAudioPlaySound>('fivepd:audio:speak').subscribe(e => this.onSpeak(e));
  }

  playSound(soundSrc: string): void {
    const sound = new Howl({
      src: [environment.production ? 'TODO' : `http://localhost:3000/audio/${soundSrc}`],
      volume: this._audioVolume
    });

    sound.play();
  }

  onSpeak(event: OnAudioPlaySound): void {
    const speechSynthesisUtterance = new SpeechSynthesisUtterance(event.Text);
    window.speechSynthesis.speak(speechSynthesisUtterance);
  }
}

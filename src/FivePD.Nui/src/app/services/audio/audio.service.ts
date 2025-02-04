import { inject, Injectable } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { OnAudioPlaySound, OnAudioSpeak } from "@models";
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
      console.log(user);
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
    const url = environment.production ? '../../Audio' : `http://localhost:3000/audio/${soundSrc}`;
    const audio = new Audio(url);
    audio.volume = this._audioVolume;
    void audio.play();
    audio.addEventListener("canplaythrough", () => {
      audio.play().catch(() => {
        window.addEventListener('click', () => {
          void audio.play();
        }, {once: true});
      });
    });
  }

  onSpeak(event: OnAudioPlaySound): void {
    const speechSynthesisUtterance = new SpeechSynthesisUtterance(event.Text);
    window.speechSynthesis.speak(speechSynthesisUtterance);
  }
}

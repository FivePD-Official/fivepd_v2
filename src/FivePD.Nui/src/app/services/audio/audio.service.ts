import { inject, Injectable } from '@angular/core';
import { EventService } from "@services/event/event.service";
import { OnAudioSpeak, OnAudioPlaySound } from "@models";
import { Howl } from 'howler';

@Injectable({
  providedIn: 'root'
})
export class AudioService {

  private _listening = false;
  private readonly _eventService = inject(EventService);

  listenToEvents(): void {
    if (this._listening) {
      return;
    }

    this._listening = true;

    this._eventService.onEvent<OnAudioSpeak>('fivepd:audio:playSound').subscribe(this.onPlaySound);
    this._eventService.onEvent<OnAudioPlaySound>('fivepd:audio:speak').subscribe(this.onSpeak);
  }

  onPlaySound(event: OnAudioSpeak): void {
    const sound = new Howl({
      src: [`./assets/sounds/${event.Sound}`],
      volume: 0.5,
    });

    sound.play();
  }

  onSpeak(event: OnAudioPlaySound): void {
    const speechSynthesisUtterance = new SpeechSynthesisUtterance(event.Text);
    window.speechSynthesis.speak(speechSynthesisUtterance);
  }
}

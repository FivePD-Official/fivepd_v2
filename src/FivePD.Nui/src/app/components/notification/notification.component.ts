import { Component, Input, OnInit } from '@angular/core';
import { Notification } from '@models';
import { IconComponent } from '../icon/icon.component';
import { NgClass, NgIf } from '@angular/common';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.sass'],
  standalone: true,
  imports: [NgClass, NgIf, IconComponent]
})
export class NotificationComponent implements OnInit {

  @Input() notification!: Notification;
  @Input() exit: boolean = false;

  constructor() {
  }

  ngOnInit(): void {
  }

}

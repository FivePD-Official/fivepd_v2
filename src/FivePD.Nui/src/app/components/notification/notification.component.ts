import { Component, Input, OnInit } from '@angular/core';
import { Notification } from '@models';

@Component({
  selector: 'app-notification',
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.sass']
})
export class NotificationComponent implements OnInit {

  @Input() notification!: Notification;
  @Input() exit: boolean = false;

  constructor() { }

  ngOnInit(): void {
  }

}

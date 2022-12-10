import { Component, OnInit } from '@angular/core';
import { User } from './_models/user';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Socialize App';

  constructor(private accountService:AccountService){}

  ngOnInit()  {
   this.setCurrentUser();
  }

  //when we load the app or open/refresh the url in browser, 
  //it sets the observable currentUserSource in accountService by checking the local storage
  setCurrentUser()
  {
    const userString = localStorage.getItem('user');
    if(!userString) return;
    const user : User = JSON.parse(userString);
    this.accountService.setCurrentUser(user);
  }
}
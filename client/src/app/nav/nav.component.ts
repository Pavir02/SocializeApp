import { Component, OnInit } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model:any = {}
  //currentUser$ : Observable<User | null> = of(null);

  constructor(public accountService : AccountService) { }

  ngOnInit(): void {
   //this.currentUser$ = this.accountService.currentUser$;
  }

  login()
  {
    this.accountService.login(this.model).subscribe(
      response => { 
                    console.log(response);
                  },
      error    => { console.log(error);        },
      ()       => { console.log("Completed");  }
    )
  }

  logout ()
  {
    this.accountService.logout();
  }

  //Note: this method is not used , because we can subscribe to the currentUser$ using async pipe in the html directly
 
  //setting the loggedIn to true or false based on the accountService.currentUser$ value which was set in 
  //appcomponent based on the local storage value when the page/application is loaded or refreshed
   /*getCurrentUser()
  {
    this.accountService.currentUser$.subscribe(
      {
      next: user => this.loggedIn= !!user,
      error: error=> console.log(error)
      }
    )
  }
  */


}

import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model:any = {}
  //currentUser$ : Observable<User | null> = of(null);

  constructor(public accountService : AccountService,
      private router:Router, private toastr:ToastrService) { }

  ngOnInit(): void {
  }

  login()
  {
    this.accountService.login(this.model).subscribe(
      {
      next: () => 
      {
        this.router.navigateByUrl('/members');
        this.model = {};
      }
      //error: error => this.toastr.error(error.error)          
      }
    )
  }

  logout ()
  {
    this.accountService.logout();
    this.router.navigateByUrl('/');
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

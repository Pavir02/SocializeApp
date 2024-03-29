import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import {map} from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable(); 

 constructor(private http:HttpClient, private presenceService:PresenceService) { }

register(model:any)
{
 return this.http.post<User>(this.baseUrl+"account/register", model).pipe(
  map(user=>
  {    
    if(user)
    {
      this.setCurrentUser(user);
      return user;
    }
  })
 )
}

  //store the user in the local storage after login and set the observable currentUserSource as well
  login(model:any)
  {
   return this.http.post<User>(this.baseUrl+"account/login", model).pipe(
    map((response:User)=>
    {
      const user = response;
      if(user)
      {
        this.setCurrentUser(user);
        return user;
      }
    })
   )
  }

  setCurrentUser(user:User)
  {
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role;
    Array.isArray(roles)? user.roles = roles : user.roles.push(roles);

    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);

    this.presenceService.createHubConnection(user);
  }

  getDecodedToken(token:string)
  {
    return JSON.parse(atob(token.split('.')[1]));
  }

  //remove the user from the local storage and currentUserSource when logout
  logout()
  {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presenceService.stopHubConnection();
  }

}

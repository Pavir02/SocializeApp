import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { User } from '../_models/user';
import { getPaginationHeader, getPaginationResult } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  membersCache = new Map();
  user: User | undefined;
  userParams : UserParams | undefined;

  constructor(private http: HttpClient, private accountService:AccountService) 
  { 
    accountService.currentUser$.pipe(take(1)).subscribe({
      next: user =>{
      if(user){
        this.userParams = new UserParams(user);
        this.user = user;
      }
    }
    })
  }

  getMembers(userParams : UserParams)
  {
    const response = this.membersCache.get(Object.values(userParams).join("-"));
    
    if(response)
    return of(response);

    let params = getPaginationHeader(userParams.pageNumber, userParams.pageSize);
    params = params.append('minAge', userParams.minAge);
    params = params.append('maxAge', userParams.maxAge);
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy); 

    return getPaginationResult<Member[]>( this.baseUrl + 'users', params, this.http)
    .pipe(
      map(response => {
         this.membersCache.set(Object.values(userParams).join("-"),response)
         return response;
      })
    )
    ;
  }
 

    //if (this.members.length > 0) return of(this.members);
    // return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      // map((members: Member[]) => {
      //   this.members = members;
      //   return members;
      // })
    // )
  

  getMember(username:string){
    //const member = this.members.find(x=> x.userName == username);

    const member =[...this.membersCache.values()]
    .reduce((arr,elem) => arr.concat(elem.result), [] )
    .find((member:Member) => member.userName === username);

    console.log(member);
    if(member) return of(member);
    return this.http.get<Member>(this.baseUrl + 'users/'+ username)
  }

  // getHttpOptions(){
  //   const userString = localStorage.getItem('user');
  //   if(!userString) return;
  //   const user = JSON.parse(userString);
  //   return {
  //   headers: new HttpHeaders({
  //       Authorization : 'Bearer ' + user.token
  //     }
  //   )
  //  }
  // }

  updateMember(member:Member){
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = {... this.members[index], ...member}
      })
    )
  }


  setMainPhoto(photoId : number){
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletPhoto(photoId : number){
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }


  addLike(userName: string)
  {
    return this.http.post(this.baseUrl + 'likes/' + userName, {});
  }

  getLikes(predicate : string, pageNumber:number, pageSize:number)
  {
    let params = getPaginationHeader(pageNumber, pageSize);
    params = params.append('predicate', predicate);
    return getPaginationResult<Member[]>(this.baseUrl + 'likes' , params, this.http)
  }

  getUserParams()
  {
    return this.userParams;
  }

  setUserParams(params: UserParams)
  {
    this.userParams = params;
  }

  resetUserParams()
  {
    if(this.user)    {
      this.userParams = new UserParams(this.user);
    }
    return this.userParams;
  }  

}

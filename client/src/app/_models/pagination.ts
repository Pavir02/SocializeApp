export interface Pagination{
    totalCount: number;
    currentPage: number;
    pageSize : number;
    totalPages : number;
}

export class PaginatedResult<T>{
    result?: T;
    pagination ?: Pagination;
}
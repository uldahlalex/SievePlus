import {useEffect, useState} from "react";
import {type Book, type CreateBookRequestDto} from "../generated-client.ts";
import useLibraryCrud from "../useLibraryCrud.ts";
import {BookDetails} from "./BookDetails.tsx";
import {useSearchParams} from "react-router";
import {SieveQueryBuilder} from "ts-sieve-query-builder";

export interface BookProps {
    book: Book,
    setAllBooks: React.Dispatch<React.SetStateAction<Book[]>>
}

export default function Books() {

    const [allBooks, setAllBooks] = useState<Book[]>([]);
    const [createBookForm, setCreateBookForm] = useState<CreateBookRequestDto>({
        pages: 1,
        title: "my amazing new book"
    });
    const libraryCrud = useLibraryCrud();
    const [searchParams, setSearchParams] = useSearchParams();

    // Helper to convert SieveModel to URLSearchParams
    const sieveModelToParams = (model: ReturnType<typeof SieveQueryBuilder.prototype.buildSieveModel>, searchValue?: string) => {
        const params = new URLSearchParams();
        if (model.filters) params.set('filters', model.filters);
        if (model.sorts) params.set('sorts', model.sorts);
        if (model.page) params.set('page', model.page.toString());
        if (model.pageSize) params.set('pageSize', model.pageSize.toString());
        if (searchValue) params.set('search', searchValue);
        return params;
    };

    // Parse current search params into a SieveQueryBuilder for type-safe querying
    const queryBuilder = SieveQueryBuilder.fromSieveModel<Book>({
        filters: searchParams.get('filters') ?? "",
        sorts: searchParams.get('sorts') ?? "",
        pageSize: Number.parseInt(searchParams.get('pageSize') ?? "2"),
        page: Number.parseInt(searchParams.get('page') ?? "1")
    });

    useEffect(() => {
        libraryCrud.getBooks(setAllBooks, queryBuilder.buildSieveModel());
    }, [searchParams])

    return <>
        <div className="p-5">
            <h2 className="text-2xl font-bold text-primary mb-4">📚 Books</h2>

            <div className="card bg-base-100 shadow-lg border border-base-300 mb-6">
                <div className="card-body p-6">
                    <h3 className="card-title text-lg mb-4 flex items-center gap-2">
                        <span>➕</span> Create New Book
                    </h3>
                    <div className="flex gap-4 items-end">
                        <div className="form-control flex-1">
                            <label className="label">
                                <span className="label-text font-medium">Book Title</span>
                            </label>
                            <input
                                value={createBookForm.title}
                                placeholder="Enter book title"
                                className="input input-bordered w-full"
                                onChange={e => setCreateBookForm({...createBookForm, title: e.target.value})}
                            />
                        </div>
                        <div className="form-control w-32">
                            <label className="label">
                                <span className="label-text font-medium">Pages</span>
                            </label>
                            <input
                                value={createBookForm.pages}
                                type="number"
                                placeholder="Pages"
                                className="input input-bordered w-full"
                                onChange={e => setCreateBookForm({...createBookForm, pages: Number.parseInt(e.target.value)})}
                            />
                        </div>
                        <button
                            className="btn btn-primary gap-2"
                            onClick={() => {
                                libraryCrud.createBook(createBookForm, setAllBooks);
                                setCreateBookForm({pages: 1, title: "My Amazing New Book"});
                            }}
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="w-4 h-4 stroke-current">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4v16m8-8H4"></path>
                            </svg>
                            Create Book
                        </button>
                    </div>
                </div>
            </div>
        </div>
        
        {/*Filter books*/}
        <div className="p-5 pt-0">
            <div className="form-control">
                <label className="label">
                    <span className="label-text font-medium">Search Books</span>
                </label>
                <input
                    placeholder="Search by title..."
                    className="input input-bordered w-full"
                    value={searchParams.get('search') ?? ''}
                    onChange={e => {
                        const searchValue = e.target.value;
                        const newQuery = SieveQueryBuilder.create<Book>()
                            .pageSize(Number.parseInt(searchParams.get('pageSize') ?? "2"))
                            .page(1); // Reset to first page on new search

                        if (searchValue) {
                            newQuery.filterContains('title', searchValue);
                        }

                        setSearchParams(sieveModelToParams(newQuery.buildSieveModel(), searchValue || undefined));
                    }}
                />
            </div>
        </div>

        <ul className="list bg-base-100 rounded-box shadow-md mx-5">
            {
                allBooks && allBooks.length > 0 && allBooks.map(b => <BookDetails key={b.id} book={b} setAllBooks={setAllBooks} />)
            }
        </ul>

        {/*Pagination*/}
        <div className="flex justify-center gap-2 p-5">
            <button
                className="btn btn-sm"
                disabled={queryBuilder.buildSieveModel().page! <= 1}
                onClick={() => {
                    const currentModel = queryBuilder.buildSieveModel();
                    const newQuery = SieveQueryBuilder.fromSieveModel<Book>(currentModel)
                        .page(currentModel.page! - 1);

                    setSearchParams(sieveModelToParams(newQuery.buildSieveModel(), searchParams.get('search') || undefined));
                }}
            >
                Previous
            </button>
            <span className="flex items-center px-4">Page {queryBuilder.buildSieveModel().page}</span>
            <button
                className="btn btn-sm"
                disabled={allBooks.length < queryBuilder.buildSieveModel().pageSize!}
                onClick={() => {
                    const currentModel = queryBuilder.buildSieveModel();
                    const newQuery = SieveQueryBuilder.fromSieveModel<Book>(currentModel)
                        .page(currentModel.page! + 1);

                    setSearchParams(sieveModelToParams(newQuery.buildSieveModel(), searchParams.get('search') || undefined));
                }}
            >
                Next
            </button>
        </div>
    </>
}

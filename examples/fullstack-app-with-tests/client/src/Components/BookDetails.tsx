import {useAtom} from "jotai";
import {type Author, type Book, type Genre, type UpdateBookRequestDto} from "../generated-client.ts";
import type {BookProps} from "./Books.tsx";
import {useEffect, useState} from "react";
import useLibraryCrud, {libraryApi} from "../useLibraryCrud.ts";

export function BookDetails(props: BookProps) {
    
    const libraryCrud = useLibraryCrud();
    const [allAuthors, setAllAuthors] = useState<Author[]>([]);
    const [allGenres, setAllGenres] = useState<Genre[]>([]);
    
    const [updateBookForm, setUpdateBookForm] = useState<UpdateBookRequestDto>({
        authorsIds: props.book.authors && props.book.authors.length > 0 ? props.book.authors.map(a => a.id) : [],
        bookIdForLookupReference: props.book.id!,
        genreId: props.book.genre?.id,
        newTitle: props.book.title!,
        newPageCount: props.book.pages!
    });

    useEffect(() => {
        libraryCrud.getAuthors(setAllAuthors, {filters: "", sorts: "", page: 1, pageSize:1000})
        libraryCrud.getGenres(setAllGenres, {filters: "", sorts: "", page: 1, pageSize:1000})
    }, [])
  

    return <li
        className="card bg-base-100 shadow-lg border border-base-300 mb-4 hover:shadow-xl transition-shadow duration-200">
        <div className="card-body p-6">
            <div className="flex justify-between items-start">
                <div className="flex-1">
                    <h3 className="card-title text-lg font-bold text-primary mb-2">{props.book.title}</h3>
                    <div className="flex flex-col gap-1">
                        <div className="badge badge-outline badge-sm">
                            📖 {props.book.pages} pages
                        </div>
                        {
                            props.book.authors && props.book.authors.length > 0 ? (
                                <div className="text-sm text-base-content/70">
                                    Written by: {props.book.authors?.map(a => a.name).join(', ')}  ✍
                                </div>
                            ) :  <div className="text-sm text-base-content/70">
                                No author has been selected yet
                            </div>
                        }
                   
                    </div>
                </div>

                <details className="dropdown dropdown-left">
                    <summary className="btn btn-square btn-outline btn-sm hover:btn-primary">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"
                             className="w-4 h-4 stroke-current">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2"
                                  d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4"></path>
                        </svg>
                    </summary>

                    <div
                        className="dropdown-content menu bg-base-100 rounded-box z-10 w-80 p-4 shadow-xl border border-base-300">
                        <div className="mb-4">
                            <h4 className="font-semibold text-lg mb-3 flex items-center gap-2">
                                <span>📝</span> Edit Book
                            </h4>

                            <div className="form-control mb-4">
                                <label className="label">
                                    <span className="label-text font-medium">Title</span>
                                </label>
                                <input
                                    className="input input-bordered w-full"
                                    value={updateBookForm.newTitle}
                                    placeholder="Enter book title"
                                    onChange={e => setUpdateBookForm({...updateBookForm, newTitle: e.target.value})}
                                />
                            </div>

                            <div className="form-control mb-4">
                                <label className="label">
                                    <span className="label-text font-medium">Pages</span>
                                </label>
                                <input
                                    className="input input-bordered w-full"
                                    type="number"
                                    value={updateBookForm.newPageCount}
                                    placeholder="Number of pages"
                                    onChange={e => setUpdateBookForm({
                                        ...updateBookForm,
                                        newPageCount: Number.parseInt(e.target.value)
                                    })}
                                />
                            </div>

                            <div className="form-control">
                                <label className="label">
                                    <span className="label-text font-medium">Authors</span>
                                </label>
                                <div className="space-y-2 max-h-32 overflow-y-auto">
                                    {allAuthors && allAuthors.length > 0 && allAuthors.map(a =>
                                        <label key={a.id}
                                               className="flex items-center gap-3 p-2 rounded-lg hover:bg-base-200 cursor-pointer">
                                            <input
                                                className="checkbox checkbox-primary checkbox-sm"
                                                type="checkbox"
                                                defaultChecked={props.book.authors.map(a => a.id).includes(a.id)}
                                                onChange={() => {
                                                    const alreadyAssigned = props.book.authors.map(a => a.id).includes(a.id!);
                                                    if (alreadyAssigned) {
                                                        const newAuthorIds = props.book.authors.map(a => a.id).filter(id => id !== a.id);
                                                        setUpdateBookForm({
                                                            ...updateBookForm,
                                                            authorsIds: newAuthorIds
                                                        });
                                                        return;
                                                    }
                                                    const newAuthorIds = [...(props.book.authors.map(a => a.id) ?? []), a.id!];
                                                    setUpdateBookForm({...updateBookForm, authorsIds: newAuthorIds});
                                                }}
                                            />
                                            <span className="text-sm">{a.name}</span>
                                        </label>
                                    )}
                                </div>
                            </div>

                            <div className="form-control">
                                <label className="label">
                                    <span className="label-text font-medium">Genre</span>
                                </label>
                                <div className="space-y-2 max-h-32 overflow-y-auto">
                                    {
                                        allGenres && allGenres.length > 0 && allGenres.map(g => {
                                            return <div key={g.id}>{g.name}
                                                <input type="radio" name="radio-1" className="radio" onChange={e => {
                                                    setUpdateBookForm({...updateBookForm, genreId: g.id!})
                                                }} defaultChecked={props.book.genre?.id == g.id}/>
                                            </div>
                                        })
                                    }
                                </div>
                            </div>


                            <div className="divider"></div>
                            <div className="flex justify-evenly">
                                <button
                                    className="btn btn-primary  gap-2"
                                    onClick={() => libraryCrud.updateBooks(updateBookForm, props.setAllBooks)}
                                >

                                    Update Book
                                </button>
                                <button
                                    className="btn btn-error  gap-2"
                                    onClick={() => libraryCrud.deleteBook(props.book.id!, props.setAllBooks)}
                                >
                                    Delete book
                                </button>
                            </div>

                        </div>
                    </div>
                </details>
            </div>
        </div>


    </li>;

}
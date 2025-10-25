import {useAtom} from "jotai";
import {type Book, type UpdateAuthorRequestDto} from "../generated-client.ts";
import {useState} from "react";
import useLibraryCrud from "../useLibraryCrud.ts";
import type {AuthorProps} from "./AuthorProps.tsx";

export function AuthorDetails(props: AuthorProps) {
    const [books, setAllBooks] = useState<Book[]>([]);
    const libraryCrud = useLibraryCrud();
    const [updateAuthorForm, setUpdateAuthorForm] = useState<UpdateAuthorRequestDto>({
        authorIdForLookup: props.author.id!,
        newName: props.author.name!,
        booksIds: props.author.books.map(b => b.id)
    });

    return <li
        className="card bg-base-100 shadow-lg border border-base-300 mb-4 hover:shadow-xl transition-shadow duration-200">
        <div className="card-body p-6">
            <div className="flex justify-between items-start">
                <div className="flex-1">
                    <h3 className="card-title text-lg font-bold text-primary mb-2">{props.author.name}</h3>
                    <div className="flex flex-col gap-1">

                        {
                            props.author.books && props.author.books.length > 0 ? (
                                <div className="text-sm text-base-content/70">
                                    📖 Books: {props.author.books.map(b => b.title).join(', ')}
                                </div>
                            ) : <div className="badge badge-outline badge-sm">
                                Has not authored any books yet
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
                                <span>✏️</span> Edit Author
                            </h4>

                            <div className="form-control mb-4">
                                <label className="label">
                                    <span className="label-text font-medium">Name</span>
                                </label>
                                <input
                                    className="input input-bordered w-full"
                                    value={updateAuthorForm.newName}
                                    placeholder="Enter author name"
                                    onChange={e => setUpdateAuthorForm({...updateAuthorForm, newName: e.target.value})}
                                />
                            </div>

                            <div className="form-control">

                                <label className="label">
                                    <span className="label-text font-medium">Books</span>
                                </label>
                                <div className="space-y-2 max-h-32 overflow-y-auto">
                                    {books.map(b =>
                                        <label key={b.id}
                                               className="flex items-center gap-3 p-2 rounded-lg hover:bg-base-200 cursor-pointer">
                                            <input
                                                className="checkbox checkbox-primary checkbox-sm"
                                                type="checkbox"
                                                defaultChecked={updateAuthorForm.booksIds.includes(b.id!)}
                                                onChange={() => {
                                                    const alreadyAssigned = updateAuthorForm.booksIds.includes(b.id!);
                                                    if (alreadyAssigned) {
                                                        const newBookIds = updateAuthorForm.booksIds.filter(id => id !== b.id);
                                                        setUpdateAuthorForm({
                                                            ...updateAuthorForm,
                                                            booksIds: newBookIds
                                                        });
                                                        return;
                                                    }
                                                    const newBookIds = [...updateAuthorForm.booksIds, b.id!];
                                                    setUpdateAuthorForm({...updateAuthorForm, booksIds: newBookIds});
                                                }}
                                            />
                                            <span className="text-sm">{b.title}</span>
                                        </label>
                                    )}
                                </div>
                            </div>

                            <div className="divider"></div>

                            <div className="flex justify-evenly">
                                <button
                                    className="btn btn-primary gap-2"
                                    onClick={() => libraryCrud.updateAuthors(updateAuthorForm, props.setAllAuthors)}
                                >
                                    Update Author
                                </button>
                                <button
                                    className="btn btn-error gap-2"
                                    onClick={() => libraryCrud.deleteAuthor(props.author.id!, props.setAllAuthors)}
                                >
                                    Delete Author
                                </button>
                            </div>
                        </div>
                    </div>
                </details>
            </div>
        </div>
    </li>;
}
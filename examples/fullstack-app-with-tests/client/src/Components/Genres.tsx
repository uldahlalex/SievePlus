import {useAtom} from "jotai";
import {useEffect, useState} from "react";
import {type CreateGenreDto, type Genre} from "../generated-client.ts";
import {GenreDetails} from "./GenreDetails.tsx";
import useLibraryCrud from "../useLibraryCrud.ts";

export interface GenreProps {
    setGenre: React.Dispatch<React.SetStateAction<Genre[]>>;
    genre: Genre
}

export default function Genres() {

    const [genres, setAllGenres] = useState<Genre[]>([]);
    const [createGenreForm, setCreateGenreForm] = useState<CreateGenreDto>({
        name: "New Genre"
    });
    const libraryCrud = useLibraryCrud();

    useEffect(() => {
        libraryCrud.getGenres(setAllGenres, {
            pageSize: 1000,
            page: 1,
            sorts: "",
            filters: ""
        })
        }, [])

    return <>
        <div className="p-5">
            <h2 className="text-2xl font-bold text-primary mb-4">🏷️ Genres</h2>

            <div className="card bg-base-100 shadow-lg border border-base-300 mb-6">
                <div className="card-body p-6">
                    <h3 className="card-title text-lg mb-4 flex items-center gap-2">
                        <span>➕</span> Create New Genre
                    </h3>
                    <div className="flex gap-4 items-end">
                        <div className="form-control flex-1">
                            <label className="label">
                                <span className="label-text font-medium">Genre Name</span>
                            </label>
                            <input
                                value={createGenreForm.name}
                                placeholder="Enter genre name"
                                className="input input-bordered w-full"
                                onChange={e => setCreateGenreForm({...createGenreForm, name: e.target.value})}
                            />
                        </div>
                        <button
                            className="btn btn-primary gap-2"
                            onClick={() => {
                                libraryCrud.createGenre(createGenreForm, setAllGenres);
                                setCreateGenreForm({name: "New Genre"});
                            }}
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="w-4 h-4 stroke-current">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 4v16m8-8H4"></path>
                            </svg>
                            Create Genre
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <ul className="list bg-base-100 rounded-box shadow-md mx-5">
            {
                genres.map(g => <GenreDetails key={g.id} genre={g} setGenre={setAllGenres} />)
            }
        </ul>
    </>
}
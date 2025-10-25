import type {
    Author,
    Book,
    CreateAuthorRequestDto,
    CreateBookRequestDto,
    CreateGenreDto, Genre, SieveModel,
    UpdateAuthorRequestDto,
    UpdateBookRequestDto,
    UpdateGenreRequestDto
} from "./generated-client.ts";
import {LibraryClient} from "./generated-client.ts";
import customCatch from "./customCatch.ts";
import toast from "react-hot-toast";
import type {Dispatch, SetStateAction} from "react";
import {resolveRefs} from "dotnet-json-refs";

const isProduction = import.meta.env.PROD;

const prod = "https://projectsolutionserver.fly.dev";
const dev = "http://localhost:5284";

const finalUrl = isProduction ? prod : dev;


export const libraryApi = new LibraryClient(finalUrl)

export default function useLibraryCrud() {

    async function updateAuthors(
        dto: UpdateAuthorRequestDto,
        setAuthors: Dispatch<SetStateAction<Author[]>>,
    ) {
        try {
            const result = resolveRefs(await libraryApi.updateAuthor(dto));
            setAuthors(prevAuthors => {
                const authors = prevAuthors || [];
                const index = authors.findIndex(a => a.id === result.id);
                if (index > -1) {
                    const duplicate = [...authors];
                    duplicate[index] = result;
                    return duplicate;
                }
                return authors;
            });

            toast.success("Author updated successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }

    async function updateBooks(
        dto: UpdateBookRequestDto,
        setBooks: Dispatch<SetStateAction<Book[]>>,
    ) {
        try {
            const result = resolveRefs(await libraryApi.updateBook(dto));
            setBooks(prevBooks => {
                const books = prevBooks || [];
                const index = books.findIndex(b => b.id === result.id);
                if (index > -1) {
                    const duplicate = [...books];
                    duplicate[index] = result;
                    return duplicate;
                }
                return books;
            });


            toast.success("Book updated successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }

    async function updateGenres(
        dto: UpdateGenreRequestDto,
        setGenres: Dispatch<SetStateAction<Genre[]>>
    ) {
        try {
            const result = resolveRefs(await libraryApi.updateGenre(dto));
            setGenres(prevGenres => {
                const genres = prevGenres || [];
                const index = genres.findIndex(g => g.id === result.id);
                if (index > -1) {
                    const duplicate = [...genres];
                    duplicate[index] = result;
                    return duplicate;
                }
                return genres;
            });
            toast.success("Genre updated successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }

    async function deleteAuthor(
        id: string,
        setAuthors: Dispatch<SetStateAction<Author[]>>
    ) {
        try {
            const result = resolveRefs(await libraryApi.deleteAuthor(id));
            setAuthors(prevAuthors => (prevAuthors || []).filter(a => a.id !== id));
            toast.success("Author deleted successfully successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }

    async function deleteBook(
        id: string,
        setBooks: Dispatch<SetStateAction<Book[]>>
    ) {
        try {
            const result = resolveRefs(await libraryApi.deleteBook(id));
            setBooks(prevBooks => (prevBooks || []).filter(b => b.id !== id));
            toast.success("Book deleted successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }

    async function deleteGenre(
        id: string,
        setGenres: Dispatch<SetStateAction<Genre[]>>
    ) {
        try {
            const result = resolveRefs(await libraryApi.deleteGenre(id));
            setGenres(prevGenres => (prevGenres || []).filter(g => g.id !== id));
            toast.success("Genre deleted successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }

    async function createAuthor(
        dto: CreateAuthorRequestDto,
        setAuthors: Dispatch<SetStateAction<Author[]>>
    ) {
        try {
            const result = resolveRefs(await libraryApi.createAuthor(dto));
            setAuthors(prevAuthors => [...(prevAuthors || []), result]);
            toast.success("Author created successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }

    async function createBook(
        dto: CreateBookRequestDto,
        setBooks: Dispatch<SetStateAction<Book[]>>
    ) {
        try {
            const result = resolveRefs(await libraryApi.createBook(dto))
            setBooks(prevBooks => [...(prevBooks || []), result]);
            toast.success("Book created successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }

    async function createGenre(
        dto: CreateGenreDto,
        setGenres: Dispatch<SetStateAction<Genre[]>>
    ) {
        try {
            const result = resolveRefs(await libraryApi.createGenre(dto));
            setGenres(prevGenres => [...(prevGenres || []), result]);
            toast.success("Genre created successfully");
            return result;
        } catch (e: any) {
            customCatch(e);
        }
    }
    
    async function getAuthors(setAuthors: Dispatch<SetStateAction<Author[]>>, sieveModel: SieveModel) {
        try {
            const result = resolveRefs(await libraryApi.getAuthors((sieveModel)));
            setAuthors(Array.isArray(result) ? result : []);
        }
        catch (e: any) {
            customCatch(e);
        }
    }

    async function getBooks(setBooks: Dispatch<SetStateAction<Book[]>>, sieveModel: SieveModel) {
        try {
            const result = resolveRefs(await libraryApi.getBooks((sieveModel)));
            setBooks(Array.isArray(result) ? result : []);
        }
        catch (e: any) {
            customCatch(e);
        }
    }

    async function getGenres(setGenres: Dispatch<SetStateAction<Genre[]>>, sieveModel: SieveModel) {
        try {
            const result = resolveRefs(await libraryApi.getGenres((sieveModel)));
            setGenres(Array.isArray(result) ? result : []);
        }
        catch (e: any) {
            customCatch(e);
        }
    }


    return {
        updateAuthors,
        updateBooks,
        updateGenres,
        deleteAuthor,
        deleteBook,
        deleteGenre,
        createAuthor,
        createBook,
        createGenre,
        getAuthors,
        getBooks,
        getGenres
    }

}
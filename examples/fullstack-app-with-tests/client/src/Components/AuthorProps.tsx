import type {Author} from "../generated-client.ts";

export interface AuthorProps {
    author: Author
    setAllAuthors: React.Dispatch<React.SetStateAction<Author[]>>
}
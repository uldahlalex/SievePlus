import {createBrowserRouter, RouterProvider} from "react-router";
import Home from "./Components/Home.tsx";
import {DevTools} from "jotai-devtools";
import 'jotai-devtools/styles.css'
import Books from "./Components/Books.tsx";
import Authors from "./Components/Authors.tsx";
import Genres from "./Components/Genres.tsx";
import {Toaster} from "react-hot-toast";


function App() {
    

return (
    <>
        <RouterProvider router={createBrowserRouter([
            {
                path: '',
                element: <Home/>,
                children: [
                    {
                        path: 'books',
                        element: <Books/>
                    },
                    {
                        path: 'authors',
                        element: <Authors/>
                    },
                    {
                        path: 'genres',
                        element: <Genres/>
                    }
                ]
            }
        ])}/>
        <DevTools/>
        <Toaster
            position="top-center"
            reverseOrder={false}
        />
    </>
)
}

export default App

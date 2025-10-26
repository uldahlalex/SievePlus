import {createBrowserRouter, RouterProvider} from "react-router";
import Home from "./Components/Home.tsx";
import {DevTools} from "jotai-devtools";
import 'jotai-devtools/styles.css'
import Computers from "./Components/Computers.tsx";
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
                        path: '',
                        element: <Computers/>
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

import {Outlet} from "react-router";

export default function Home() {

    return <div className="min-h-screen bg-base-200">
        <Outlet />
    </div>
}

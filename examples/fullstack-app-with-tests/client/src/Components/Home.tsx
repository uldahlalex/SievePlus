import {Outlet, useNavigate} from "react-router";

export default function Home() {
    
    const navigate = useNavigate();
    
    return <>

        
        <Outlet />

        <div className="dock">
            <button className="btn btn-outline" onClick={() => navigate('books')}>
                <span className="dock-label">Books📚</span>
            </button>

            <button className="btn btn-outline" onClick={() => navigate('authors')}>
                <span className="dock-label">Authors👫</span>
            </button>

            <button className="btn btn-outline" onClick={() => navigate('genres')}>
                <span className="dock-label">Genres✍️</span>
            </button>
        </div>
    </>
}
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import NotificationBell from './NotificationBell'

export default function Navbar() {
  const { user, logoutUser, displayName, initials, isUnit, isAdmin, managedOrgId } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logoutUser()
    navigate('/')
  }

  return (
    <nav className="bg-[#16171d] border-b border-[#2e303a] sticky top-0 z-50">
      <div className="max-w-6xl mx-auto px-4 flex items-center justify-between h-14">
        <div className="flex items-center gap-8">
          <Link to="/" className="font-bold text-white text-lg">
            VolunteerHQ
          </Link>
          <div className="hidden md:flex items-center gap-6 text-sm text-gray-400">
            <Link to="/fundraisers" className="hover:text-white transition-colors">Збори</Link>
            <Link to="/organizations" className="hover:text-white transition-colors">Організації</Link>
            <Link to="/units" className="hover:text-white transition-colors">Підрозділи</Link>
            {managedOrgId && (
              <Link to={`/organizations/${managedOrgId}/manage`} className="text-yellow-400 hover:text-yellow-300 transition-colors">
                Керування
              </Link>
            )}
            {isAdmin && (
              <Link to="/admin" className="text-yellow-400 hover:text-yellow-300 transition-colors">
                Адмін
              </Link>
            )}
          </div>
        </div>

        <div className="flex items-center gap-3">
          {user ? (
            <>
              {isUnit ? (
                <div className="flex items-center gap-2">
                  <div className="w-8 h-8 bg-green-400/20 rounded-full flex items-center justify-center text-green-400 text-sm font-bold shrink-0">
                    🎖
                  </div>
                  <span className="text-sm text-gray-300 hidden md:block">
                    {displayName}
                  </span>
                </div>
              ) : (
                <>
                  <NotificationBell />
                  <Link
                    to="/messages"
                    className="w-9 h-9 flex items-center justify-center rounded-lg hover:bg-[#2e303a] transition-colors text-gray-400 hover:text-white"
                    title="Повідомлення"
                  >
                    ✉
                  </Link>
                  <Link
                    to="/profile"
                    className="flex items-center gap-2 hover:opacity-80 transition-opacity"
                  >
                    <div className="w-8 h-8 bg-yellow-400 rounded-full flex items-center justify-center text-black text-xs font-bold shrink-0">
                      {initials?.toUpperCase() || '?'}
                    </div>
                    <span className="text-sm text-gray-300 hidden md:block">
                      {displayName}
                    </span>
                  </Link>
                </>
              )}
              <button
                onClick={handleLogout}
                className="text-sm text-gray-500 hover:text-white transition-colors ml-1"
              >
                Вийти
              </button>
            </>
          ) : (
            <>
              <Link
                to="/login"
                className="text-sm border border-[#2e303a] text-white px-4 py-1.5 rounded-lg hover:border-gray-500 transition-colors"
              >
                Увійти
              </Link>
              <Link
                to="/register"
                className="text-sm bg-yellow-400 text-black font-semibold px-4 py-1.5 rounded-lg hover:bg-yellow-300 transition-colors"
              >
                Реєстрація
              </Link>
            </>
          )}
        </div>
      </div>
    </nav>
  )
}

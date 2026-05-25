import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { loginMilitaryUnit } from '../api/militaryUnits'
import { useAuth } from '../context/AuthContext'

export default function UnitLoginPage() {
  const [form, setForm] = useState({ login: '', password: '' })
  const [loading, setLoading] = useState(false)
  const { loginUnit } = useAuth()
  const navigate = useNavigate()

  const handleChange = (e) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      const res = await loginMilitaryUnit(form)
      loginUnit({
        unitId: res.data.unitId,
        unitName: res.data.unitName,
        role: 'MilitaryUnit',
      }, res.data.token)
      navigate('/')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Невірний логін або пароль')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <Link to="/" className="text-white font-bold text-2xl">VolunteerHQ</Link>
          <div className="inline-flex items-center gap-2 bg-green-400/10 border border-green-400/20 rounded-full px-3 py-1 mt-3">
            <span className="text-green-400 text-xs font-medium">🎖 ВІЙСЬКОВИЙ ПІДРОЗДІЛ</span>
          </div>
        </div>

        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-gray-400 text-sm mb-2">Логін</label>
              <input
                type="text"
                name="login"
                value={form.login}
                onChange={handleChange}
                placeholder="unit_login"
                required
                autoComplete="username"
                className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
              />
            </div>
            <div>
              <label className="block text-gray-400 text-sm mb-2">Пароль</label>
              <input
                type="password"
                name="password"
                value={form.password}
                onChange={handleChange}
                placeholder="••••••••"
                required
                autoComplete="current-password"
                className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
              />
            </div>
            <button
              type="submit"
              disabled={loading}
              className="w-full bg-yellow-400 text-black font-semibold py-3 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Завантаження...' : 'Увійти'}
            </button>
          </form>

          <p className="text-center text-gray-500 text-sm mt-6">
            Звичайний користувач?{' '}
            <Link to="/login" className="text-yellow-400 hover:text-yellow-300 transition-colors">
              Увійти
            </Link>
          </p>
        </div>

        <p className="text-center text-gray-600 text-xs mt-4 leading-relaxed">
          Реєстрація військових підрозділів виконується адміністрацією платформи.
        </p>
      </div>
    </div>
  )
}

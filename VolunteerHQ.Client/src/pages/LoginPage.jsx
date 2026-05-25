import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { login } from '../api/auth'
import { useAuth } from '../context/AuthContext'

export default function LoginPage() {
  const [form, setForm] = useState({ email: '', password: '' })
  const [loading, setLoading] = useState(false)
  const { loginUser } = useAuth()
  const navigate = useNavigate()

  const handleChange = (e) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      const res = await login(form)
      loginUser({ userId: res.data.userId, email: form.email, role: res.data.role, firstName: res.data.firstName, secondName: res.data.secondName }, res.data.token)
      navigate('/')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Невірний email або пароль')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <Link to="/" className="text-white font-bold text-2xl">VolunteerHQ</Link>
          <p className="text-gray-500 mt-2 text-sm">Увійдіть у свій акаунт</p>
        </div>

        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-gray-400 text-sm mb-2">Email</label>
              <input
                type="email"
                name="email"
                value={form.email}
                onChange={handleChange}
                placeholder="you@example.com"
                required
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
            Немає акаунту?{' '}
            <Link to="/register" className="text-yellow-400 hover:text-yellow-300 transition-colors">
              Зареєструватись
            </Link>
          </p>
          <p className="text-center text-gray-600 text-xs mt-3">
            <Link to="/units/login" className="hover:text-gray-400 transition-colors">
              🎖 Вхід для військового підрозділу
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}

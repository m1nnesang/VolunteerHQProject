import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { register } from '../api/auth'
import { useAuth } from '../context/AuthContext'

const validate = (form) => {
  const errors = {}
  if (!form.firstName.trim()) errors.firstName = "Ім'я обов'язкове"
  else if (form.firstName.trim().length < 2) errors.firstName = "Мін. 2 символи"
  else if (!/^[a-zA-Zа-яА-ЯіІїЇєЄ'-]+$/.test(form.firstName.trim())) errors.firstName = "Тільки літери"

  if (!form.secondName.trim()) errors.secondName = "Прізвище обов'язкове"
  else if (form.secondName.trim().length < 2) errors.secondName = "Мін. 2 символи"
  else if (!/^[a-zA-Zа-яА-ЯіІїЇєЄ'-]+$/.test(form.secondName.trim())) errors.secondName = "Тільки літери"

  if (!form.email.trim()) errors.email = 'Email обов\'язковий'
  else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) errors.email = 'Невірний формат email'

  if (form.phoneNumber && !/^\+380\d{9}$/.test(form.phoneNumber))
    errors.phoneNumber = 'Формат: +380XXXXXXXXX'

  if (!form.password) errors.password = 'Пароль обов\'язковий'
  else if (form.password.length < 8) errors.password = 'Мін. 8 символів'

  if (form.birthDate) {
    const birth = new Date(form.birthDate)
    const age = Math.floor((Date.now() - birth) / (365.25 * 24 * 60 * 60 * 1000))
    if (age < 14) errors.birthDate = 'Мін. вік — 14 років'
    else if (age > 120) errors.birthDate = 'Невірна дата'
  }

  return errors
}

function Field({ label, error, children }) {
  return (
    <div>
      <label className="block text-gray-400 text-sm mb-2">{label}</label>
      {children}
      {error && <p className="text-red-400 text-xs mt-1">{error}</p>}
    </div>
  )
}

export default function RegisterPage() {
  const [form, setForm] = useState({
    firstName: '',
    secondName: '',
    email: '',
    password: '',
    phoneNumber: '',
    birthDate: '',
  })
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(false)
  const { loginUser } = useAuth()
  const navigate = useNavigate()

  const handleChange = (e) => {
    const { name, value } = e.target
    setForm(prev => ({ ...prev, [name]: value }))
    if (errors[name]) setErrors(prev => ({ ...prev, [name]: '' }))
  }

  const handleBlur = (e) => {
    const { name } = e.target
    const fieldErrors = validate(form)
    if (fieldErrors[name]) setErrors(prev => ({ ...prev, [name]: fieldErrors[name] }))
  }

  const inputClass = (field) =>
    `w-full bg-[#111218] border rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none transition-colors ${
      errors[field] ? 'border-red-500 focus:border-red-400' : 'border-[#2e303a] focus:border-yellow-400'
    }`

  const handleSubmit = async (e) => {
    e.preventDefault()
    const validationErrors = validate(form)
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors)
      return
    }
    setLoading(true)
    try {
      const res = await register(form)
      loginUser({ userId: res.data.userId, email: form.email, role: res.data.role, firstName: res.data.firstName, secondName: res.data.secondName }, res.data.token)
      navigate('/')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка реєстрації')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <Link to="/" className="text-white font-bold text-2xl">VolunteerHQ</Link>
          <p className="text-gray-500 mt-2 text-sm">Створіть акаунт волонтера</p>
        </div>

        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8">
          <form onSubmit={handleSubmit} onBlur={handleBlur} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <Field label="Ім'я" error={errors.firstName}>
                <input
                  type="text"
                  name="firstName"
                  value={form.firstName}
                  onChange={handleChange}
                  placeholder="Іван"
                  className={inputClass('firstName')}
                />
              </Field>
              <Field label="Прізвище" error={errors.secondName}>
                <input
                  type="text"
                  name="secondName"
                  value={form.secondName}
                  onChange={handleChange}
                  placeholder="Шевченко"
                  className={inputClass('secondName')}
                />
              </Field>
            </div>

            <Field label="Email" error={errors.email}>
              <input
                type="text"
                name="email"
                value={form.email}
                onChange={handleChange}
                placeholder="you@example.com"
                className={inputClass('email')}
              />
            </Field>

            <Field label="Телефон" error={errors.phoneNumber}>
              <input
                type="tel"
                name="phoneNumber"
                value={form.phoneNumber}
                onChange={handleChange}
                placeholder="+380501234567"
                className={inputClass('phoneNumber')}
              />
            </Field>

            <Field label="Дата народження" error={errors.birthDate}>
              <input
                type="date"
                name="birthDate"
                value={form.birthDate}
                onChange={handleChange}
                max={new Date().toISOString().split('T')[0]}
                className={inputClass('birthDate')}
              />
            </Field>

            <Field label="Пароль" error={errors.password}>
              <input
                type="password"
                name="password"
                value={form.password}
                onChange={handleChange}
                placeholder="Мін. 8 символів"
                className={inputClass('password')}
              />
            </Field>

            <button
              type="submit"
              disabled={loading}
              className="w-full bg-yellow-400 text-black font-semibold py-3 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed mt-2"
            >
              {loading ? 'Завантаження...' : 'Зареєструватись'}
            </button>
          </form>

          <p className="text-center text-gray-500 text-sm mt-6">
            Вже є акаунт?{' '}
            <Link to="/login" className="text-yellow-400 hover:text-yellow-300 transition-colors">
              Увійти
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}

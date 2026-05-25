import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import { AuthProvider } from './context/AuthContext'
import Navbar from './components/Navbar'
import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import FundraisersPage from './pages/FundraisersPage'
import FundraiserDetailPage from './pages/FundraiserDetailPage'
import ProfilePage from './pages/ProfilePage'
import OrganizationsPage from './pages/OrganizationsPage'
import OrganizationDetailPage from './pages/OrganizationDetailPage'
import MilitaryUnitsPage from './pages/MilitaryUnitsPage'
import MilitaryUnitDetailPage from './pages/MilitaryUnitDetailPage'
import UnitLoginPage from './pages/UnitLoginPage'
import CreateFundraiserPage from './pages/CreateFundraiserPage'
import CreateOrganizationRequestPage from './pages/CreateOrganizationRequestPage'
import SubscriptionsPage from './pages/SubscriptionsPage'
import OrganizationManagePage from './pages/OrganizationManagePage'
import AdminPage from './pages/AdminPage'
import DonateViaCodePage from './pages/DonateViaCodePage'
import MessagesPage from './pages/MessagesPage'
import MessagesInboxPage from './pages/MessagesInboxPage'
import ProtectedRoute from './components/ProtectedRoute'

function Layout({ children }) {
  return (
    <>
      <Navbar />
      {children}
    </>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <Toaster
        position="top-right"
        toastOptions={{
          style: {
            background: '#1c1d24',
            color: '#e5e7eb',
            border: '1px solid #2e303a',
            borderRadius: '10px',
            fontSize: '14px',
          },
          success: { iconTheme: { primary: '#facc15', secondary: '#000' } },
          error: { iconTheme: { primary: '#f87171', secondary: '#fff' } },
        }}
      />
      <BrowserRouter>
        <Routes>
          {/* Сторінки без navbar */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/units/login" element={<UnitLoginPage />} />

          {/* Сторінки з navbar */}
          <Route path="/" element={<Layout><HomePage /></Layout>} />
          <Route path="/fundraisers" element={<Layout><FundraisersPage /></Layout>} />
          <Route path="/fundraisers/:id" element={<Layout><FundraiserDetailPage /></Layout>} />
          <Route path="/profile" element={<Layout><ProtectedRoute><ProfilePage /></ProtectedRoute></Layout>} />
          <Route path="/organizations" element={<Layout><OrganizationsPage /></Layout>} />
          <Route path="/organizations/:id" element={<Layout><OrganizationDetailPage /></Layout>} />
          <Route path="/units" element={<Layout><MilitaryUnitsPage /></Layout>} />
          <Route path="/units/:id" element={<Layout><MilitaryUnitDetailPage /></Layout>} />
          <Route path="/fundraisers/create" element={<Layout><ProtectedRoute requireUnit><CreateFundraiserPage /></ProtectedRoute></Layout>} />
          <Route path="/organizations/create" element={<Layout><ProtectedRoute><CreateOrganizationRequestPage /></ProtectedRoute></Layout>} />
          <Route path="/subscriptions" element={<Layout><ProtectedRoute><SubscriptionsPage /></ProtectedRoute></Layout>} />
          <Route path="/organizations/:id/manage" element={<Layout><ProtectedRoute><OrganizationManagePage /></ProtectedRoute></Layout>} />
          <Route path="/admin" element={<Layout><ProtectedRoute><AdminPage /></ProtectedRoute></Layout>} />
          <Route path="/donate/:uniqueCode" element={<DonateViaCodePage />} />
          <Route path="/messages" element={<Layout><ProtectedRoute><MessagesInboxPage /></ProtectedRoute></Layout>} />
          <Route path="/messages/:userId" element={<Layout><ProtectedRoute><MessagesPage /></ProtectedRoute></Layout>} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

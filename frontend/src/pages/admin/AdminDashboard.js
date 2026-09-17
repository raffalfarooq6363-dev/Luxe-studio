import React, { useState, useEffect } from 'react';
import { Routes, Route, Link, Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import adminService from '../../services/adminService';
import { servicesData } from '../../data/store';
import './AdminDashboard.css';

const AdminDashboard = () => {
  const { user } = useAuth();
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState('overview');

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      const data = await adminService.getDashboard();
      setDashboardData(data);
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
      // Sample data for demonstration
      setDashboardData({
        totalUsers: 150,
        totalAppointments: 320,
        totalRevenue: 45000,
        pendingAppointments: 12,
      });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="dashboard admin-dashboard">
      <div className="dashboard-sidebar">
        <div className="dashboard-user">
          <div className="user-avatar admin-avatar">
            {user?.profileImageUrl ? <img src={user.profileImageUrl} alt={`${user.firstName} ${user.lastName}`} /> : <>{user?.firstName?.[0]}{user?.lastName?.[0]}</>}
          </div>
          <h3>{user?.firstName} {user?.lastName}</h3>
          <p className="role-badge">Admin</p>
        </div>

        <nav className="dashboard-nav">
          <Link
            to="/admin"
            className={activeTab === 'overview' ? 'active' : ''}
            onClick={() => setActiveTab('overview')}
          >
            📊 Dashboard
          </Link>
          <Link
            to="/admin/bookings"
            className={activeTab === 'bookings' ? 'active' : ''}
            onClick={() => setActiveTab('bookings')}
          >
            📅 Bookings
          </Link>
          <Link
            to="/admin/users"
            className={activeTab === 'users' ? 'active' : ''}
            onClick={() => setActiveTab('users')}
          >
            👥 Users
          </Link>
          <Link
            to="/admin/services"
            className={activeTab === 'services' ? 'active' : ''}
            onClick={() => setActiveTab('services')}
          >
            💼 Services
          </Link>
          <Link
            to="/admin/practitioners"
            className={activeTab === 'practitioners' ? 'active' : ''}
            onClick={() => setActiveTab('practitioners')}
          >
            👨‍⚕️ Practitioners
          </Link>
          <Link
            to="/admin/payments"
            className={activeTab === 'payments' ? 'active' : ''}
            onClick={() => setActiveTab('payments')}
          >
            💰 Payments
          </Link>
          <Link
            to="/admin/reports"
            className={activeTab === 'reports' ? 'active' : ''}
            onClick={() => setActiveTab('reports')}
          >
            📈 Reports
          </Link>
        </nav>
      </div>

      <div className="dashboard-content">
        <Routes>
          <Route path="/" element={
            <div>
              <h1>Admin Dashboard</h1>
              {loading ? (
                <p>Loading...</p>
              ) : (
                <>
                  <div className="dashboard-stats">
                    <div className="stat-card stat-primary">
                      <h3>Total Users</h3>
                      <p className="stat-number">{dashboardData?.totalUsers || 0}</p>
                    </div>
                    <div className="stat-card stat-success">
                      <h3>Total Appointments</h3>
                      <p className="stat-number">{dashboardData?.totalAppointments || 0}</p>
                    </div>
                    <div className="stat-card stat-warning">
                      <h3>Revenue</h3>
                      <p className="stat-number">${dashboardData?.totalRevenue || 0}</p>
                    </div>
                    <div className="stat-card stat-info">
                      <h3>Pending</h3>
                      <p className="stat-number">{dashboardData?.pendingAppointments || 0}</p>
                    </div>
                  </div>

                  <div className="admin-quick-actions">
                    <h2>Quick Actions</h2>
                    <div className="action-buttons">
                      <Link to="/admin/bookings" className="action-btn">
                        📅 Manage Bookings
                      </Link>
                      <Link to="/admin/services" className="action-btn">
                        ➕ Add Service
                      </Link>
                      <Link to="/admin/users" className="action-btn">
                        👥 View Users
                      </Link>
                      <Link to="/admin/reports" className="action-btn">
                        📊 View Reports
                      </Link>
                    </div>
                  </div>
                </>
              )}
            </div>
          } />
          <Route path="bookings" element={<BookingsPanel />} />
          <Route path="users" element={<UsersPanel />} />
          <Route path="services" element={<ServicesPanel />} />
          <Route path="practitioners" element={<UsersPanel practitionersOnly />} />
          <Route path="payments" element={<PaymentsPanel />} />
          <Route path="reports" element={<ReportsPanel />} />
          <Route path="*" element={<Navigate to="/admin" replace />} />
        </Routes>
      </div>
    </div>
  );
};

const PanelHeader = ({ title, description, action }) => (
  <div className="admin-panel-header">
    <div><h1>{title}</h1><p>{description}</p></div>
    {action}
  </div>
);

const Loading = () => <div className="admin-loading">Loading...</div>;
const Empty = ({ children }) => <div className="admin-empty">{children}</div>;
const ErrorMessage = ({ children }) => <p className="dashboard-error">{children}</p>;
const formatDate = (value) => value ? new Date(value).toLocaleDateString() : '-';

const BookingsPanel = () => {
  const [bookings, setBookings] = useState([]);
  const [status, setStatus] = useState('');
  const [query, setQuery] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const load = () => {
    setLoading(true);
    adminService.getBookings({ page: 1, pageSize: 100, status: status || undefined })
      .then((data) => setBookings(data.bookings || []))
      .catch(() => setError('Unable to load bookings.'))
      .finally(() => setLoading(false));
  };
  useEffect(load, [status]);
  const filtered = bookings.filter((booking) => `${booking.userName} ${booking.userEmail} ${booking.serviceName}`.toLowerCase().includes(query.toLowerCase()));
  const updateStatus = async (id, nextStatus) => {
    const reason = nextStatus === 'Cancelled' ? window.prompt('Why is this booking being rejected?') : '';
    if (nextStatus === 'Cancelled' && !reason?.trim()) return;
    try { await adminService.updateBookingStatus(id, nextStatus, reason); load(); } catch { setError('Unable to update booking status.'); }
  };
  return <div><PanelHeader title="Manage Bookings" description="Review reservations and update appointment status." action={<button className="admin-refresh-btn" onClick={load}>Refresh</button>} />{error && <ErrorMessage>{error}</ErrorMessage>}<div className="admin-toolbar"><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search customer or service" /><select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">All statuses</option><option>Pending</option><option>Confirmed</option><option>Completed</option><option>Cancelled</option></select></div>{loading ? <Loading /> : <div className="admin-table-wrap"><table className="admin-table"><thead><tr><th>Customer</th><th>Service</th><th>Date</th><th>Amount</th><th>Status</th><th>Update</th></tr></thead><tbody>{filtered.map((booking) => <tr key={booking.id}><td><strong>{booking.userName}</strong><small>{booking.userEmail}</small></td><td>{booking.serviceName}</td><td>{formatDate(booking.appointmentDate)}<small>{booking.startTime}</small></td><td>${booking.totalAmount}</td><td><span className={`status-pill ${booking.status?.toLowerCase()}`}>{booking.status}</span></td><td><select value={booking.status} onChange={(event) => updateStatus(booking.id, event.target.value)}><option>Pending</option><option>Confirmed</option><option>Completed</option><option>Cancelled</option></select></td></tr>)}</tbody></table>{!filtered.length && <Empty>No bookings match your filters.</Empty>}</div>}</div>;
};

const UsersPanel = ({ practitionersOnly = false }) => {
  const [users, setUsers] = useState([]);
  const [query, setQuery] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const load = () => { setLoading(true); adminService.getUsers({ page: 1, pageSize: 100, role: practitionersOnly ? 'Practitioner' : undefined, searchTerm: query || undefined }).then((data) => setUsers(data.items || [])).catch(() => setError('Unable to load users.')).finally(() => setLoading(false)); };
  useEffect(load, [practitionersOnly, query]);
  const toggleStatus = async (item) => { try { await adminService.updateUserStatus(item.id, !item.isActive); load(); } catch { setError('Unable to update user status.'); } };
  return <div><PanelHeader title={practitionersOnly ? 'Practitioners' : 'User Management'} description="Search users and manage account access." action={<button className="admin-refresh-btn" onClick={load}>Refresh</button>} />{error && <ErrorMessage>{error}</ErrorMessage>}<div className="admin-toolbar"><input value={query} onChange={(event) => setQuery(event.target.value)} onKeyDown={(event) => event.key === 'Enter' && load()} placeholder="Search by name or email" /><button className="admin-refresh-btn" onClick={load}>Search</button></div>{loading ? <Loading /> : <div className="admin-table-wrap"><table className="admin-table"><thead><tr><th>Name</th><th>Email</th><th>Role</th><th>Joined</th><th>Status</th><th>Action</th></tr></thead><tbody>{users.map((item) => <tr key={item.id}><td><strong>{item.fullName || `${item.firstName} ${item.lastName}`}</strong><small>{item.phoneNumber || 'No phone number'}</small></td><td>{item.email}</td><td>{item.role}</td><td>{formatDate(item.createdAt)}</td><td><span className={`status-pill ${item.isActive ? 'active' : 'inactive'}`}>{item.isActive ? 'Active' : 'Inactive'}</span></td><td><button className="table-action-btn" onClick={() => toggleStatus(item)}>{item.isActive ? 'Deactivate' : 'Activate'}</button></td></tr>)}</tbody></table>{!users.length && <Empty>No users found.</Empty>}</div>}</div>;
};

const ServicesPanel = () => {
  const [services, setServices] = useState(servicesData);
  const [form, setForm] = useState({ name: '', description: '', price: '', durationMinutes: 60, categoryId: 1, imageUrl: '' });
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const createService = async (event) => { event.preventDefault(); setMessage(''); setError(''); try { await adminService.createService({ ...form, price: Number(form.price), durationMinutes: Number(form.durationMinutes), categoryId: Number(form.categoryId) }); setMessage('Service created successfully.'); setServices([{ ...form, id: Date.now(), price: Number(form.price) }, ...services]); setForm({ name: '', description: '', price: '', durationMinutes: 60, categoryId: 1, imageUrl: '' }); } catch { setError('Unable to create service. Check the category ID and API connection.'); } };
  return <div><PanelHeader title="Services" description="Review the treatment menu and add new services." />{message && <p className="dashboard-success">{message}</p>}{error && <ErrorMessage>{error}</ErrorMessage>}<div className="admin-grid-two"><section className="admin-section-card"><div className="section-heading"><h2>Current menu</h2><span>{services.length} services</span></div>{services.map((service) => <div className="service-admin-row" key={service.id}>{service.imageUrl && <img src={service.imageUrl} alt="" /> }<div><strong>{service.name}</strong><span>{service.durationMinutes} min · {service.category}</span></div><strong>${service.price}</strong></div>)}</section><form className="admin-section-card admin-form" onSubmit={createService}><h2>Add service</h2><label>Name<input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required /></label><label>Description<textarea value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} required /></label><label>Image URL<input type="url" placeholder="https://example.com/service-image.jpg" value={form.imageUrl} onChange={(event) => setForm({ ...form, imageUrl: event.target.value })} /></label>{form.imageUrl && <img className="service-image-preview" src={form.imageUrl} alt="Service preview" />}<div className="form-row"><label>Price<input type="number" min="0" value={form.price} onChange={(event) => setForm({ ...form, price: event.target.value })} required /></label><label>Minutes<input type="number" min="15" value={form.durationMinutes} onChange={(event) => setForm({ ...form, durationMinutes: event.target.value })} required /></label></div><label>Category ID<input type="number" min="1" value={form.categoryId} onChange={(event) => setForm({ ...form, categoryId: event.target.value })} required /></label><button className="admin-primary-btn" type="submit">Create Service</button></form></div></div>;
};

const PaymentsPanel = () => { const [data, setData] = useState(null); useEffect(() => { adminService.getDashboard().then(setData).catch(() => setData({ recentPayments: [] })); }, []); return <div><PanelHeader title="Payments" description="Review recent payment activity." />{data?.recentPayments?.length ? <section className="admin-section-card"><div className="admin-table-wrap"><table className="admin-table"><thead><tr><th>Payment</th><th>Method</th><th>Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>{data.recentPayments.map((payment) => <tr key={payment.id}><td>#{payment.id}</td><td>{payment.paymentMethod}</td><td>{formatDate(payment.paymentDate)}</td><td><span className={`status-pill ${payment.status?.toLowerCase()}`}>{payment.status}</span></td><td>${payment.totalAmount}</td></tr>)}</tbody></table></div></section> : <Empty>No payment activity found.</Empty>}</div>; };

const ReportsPanel = () => { const [data, setData] = useState(null); useEffect(() => { adminService.getAnalytics().then(setData).catch(() => setData(null)); }, []); return <div><PanelHeader title="Reports & Analytics" description="Performance insights for the last 30 days." />{data ? <div className="admin-grid-two">{['appointmentStats', 'revenueStats', 'serviceStats', 'customerStats'].map((key) => <section className="admin-section-card" key={key}><h2>{key.replace('Stats', ' statistics')}</h2>{data[key] && Object.keys(data[key]).length ? Object.entries(data[key]).map(([label, value]) => <div className="metric-row" key={label}><span>{label}</span><strong>{String(value)}</strong></div>) : <p className="admin-muted">No data available.</p>}</section>)}</div> : <Empty>Analytics are not available yet.</Empty>}</div>; };

export default AdminDashboard;
